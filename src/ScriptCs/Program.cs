using McMaster.Extensions.CommandLineUtils;
using ScriptCs.Contracts;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ScriptCs
{
    internal static class Program
    {
        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomain)]
        private static int Main(string[] args)
        {
            //args = args.Skip(2).ToArray();
            ProfileOptimizationShim.SetProfileRoot(Path.GetDirectoryName(typeof(Program).Assembly.Location));
            ProfileOptimizationShim.StartProfile(typeof(Program).Assembly.GetName().Name + ".profile");

            var nonScriptArgs = args.TakeWhile(arg => arg != "--").ToArray();
            var scriptArgs = args.Skip(nonScriptArgs.Length + 1).ToArray();

            var app = new CommandLineApplication(throwOnUnexpectedArg: true)
            {
                ExtendedHelpText = "Usage: scriptcs options",
                OptionsComparison = StringComparison.OrdinalIgnoreCase
            };

            var script = app.Argument("script", "Script file name, must be specified first");

            var scriptNameFallback = app.Option("--scriptname | -script", "Alternative way to pass a script filename", CommandOptionType.NoValue);
            var repl = app.Option("--repl | -r", "Launch REPL mode when running script. To just launch REPL, simply omit the 'script' argument", CommandOptionType.NoValue);
            var eval = app.Option("--eval | -e", "Code to immediately evaluate", CommandOptionType.SingleValue);
            var configFile = app.Option("--config | -co", "Defines config file name", CommandOptionType.SingleValue);
            var debug = app.Option("--debug | -d", "Emits PDB symbols allowing for attaching a Visual Studio debugger", CommandOptionType.NoValue);
            var version = app.Option("--version | -v", "Outputs version information", CommandOptionType.NoValue);
            var cache = app.Option("--cache | -c", "Flag which determines whether to run in memory or from a .dll", CommandOptionType.NoValue);
            var logLevel = app.Option<LogLevel?>("--loglevel | -log", "Flag which defines the log level used", CommandOptionType.SingleValue);
            var watch = app.Option("--watch | -w", "Watch the script file and reload it when changed", CommandOptionType.NoValue);
            var modules = app.Option("--modules | -m", "Specify modules to load (comma separated)", CommandOptionType.SingleValue);
            var output = app.Option("--output | -o", "Write all console output to the specified file", CommandOptionType.SingleValue);

            app.HelpOption("-Help | -?");

            app.Command("install", c =>
            {
                var package = c.Argument("package", "Specific package to install, otherwise installs and restores packages which are specified in packages.config");
                var allowPrerelease = c.Option("--allowprerelease | -pre", "Allows installation of packages' prelease versions", CommandOptionType.NoValue);
                var packageVersion = c.Option("--packageversion | -p", "Defines the version of the package to install", CommandOptionType.SingleValue);
                var save = c.Option("--save | -s", "Creates a packages.config file based on the packages directory", CommandOptionType.NoValue);
                var clean = c.Option("--clean | -cl", "Cleans installed packages from working directory", CommandOptionType.NoValue);
                var global = c.Option("--global | -g", "Installs and restores global packages which are specified in packages.config", CommandOptionType.NoValue);

                c.OnExecute(() =>
                {
                    var configMask = new ConfigMask
                    {
                        Repl = false,
                        Global = global.HasValue() ? true : (bool?)null,
                        Install = package.Value ?? string.Empty, // needed to trigger install of all packages!
                        PackageVersion = packageVersion.Value(),
                        AllowPreRelease = allowPrerelease.HasValue() ? true : (bool?)null,
                        Save = save.HasValue() ? true : (bool?)null,
                        Clean = clean.HasValue() ? true : (bool?)null,
                        Output = output.Value(),
                        Debug = debug.HasValue() ? true : (bool?)null,
                        LogLevel = logLevel.ParsedValue
                    };

                    return Application.Run(Config.Create(configFile.Value(), configMask), scriptArgs);
                });
            });

            app.OnExecute(() =>
            {
                if (configFile.HasValue() && !File.Exists(configFile.Value()))
                {
                    Console.WriteLine("The specified config file does not exist.");
                    return 1;
                }

                if (version.HasValue())
                {
                    VersionWriter.Write();
                    return 0;
                }

                var configMask = new ConfigMask
                {
                    Repl = repl.HasValue() ? true : (bool?)null,
                    ScriptName = scriptNameFallback.HasValue() ? scriptNameFallback.Value() : script.Value,
                    Debug = debug.HasValue() ? true : (bool?)null,
                    Eval = eval.Value(),
                    Cache = cache.HasValue() ? true : (bool?)null,
                    Watch = watch.HasValue() ? true : (bool?)null,
                    LogLevel = logLevel.ParsedValue,
                    Modules = modules.Value(),
                    Output = output.Value()
                };

                return Application.Run(Config.Create(configFile.Value(), configMask), scriptArgs);
            });

            try
            {
                return app.Execute(nonScriptArgs);
            }
            catch (CommandParsingException)
            {
                app.ShowHelp();
                return 1;
            }
        }
    }
}
