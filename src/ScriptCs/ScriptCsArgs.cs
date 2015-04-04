using System;
using System.Collections.Generic;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [ArgExample(
        "scriptcs server.csx -logLevel debug",
        "Executes the 'server.csx' script and displays detailed log messages. Useful for debugging.")]
    public class ScriptCsArgs
    {
        [ArgDescription("Launch REPL mode when running script. To just launch REPL, simply omit the 'script' argument.")]
        public bool Repl { get; set; }

        [ArgPosition(0)]
        [ArgShortcut("script")]
        [ArgDescription("Script file name, must be specified first")]
        public string ScriptName { get; set; }

        [ArgShortcut("?")]
        [ArgDescription("Displays help")]
        public bool Help { get; set; }

        [ArgDescription("Emits PDB symbols allowing for attaching a Visual Studio debugger")]
        public bool Debug { get; set; }

        [ArgDescription("Flag which determines whether to run in memory or from a .dll")]
        public bool Cache { get; set; }

        [ArgShortcut("log")]
        [ArgDescription("Flag which defines the log level used.")]
        public LogLevel? LogLevel { get; set; }

        [ArgDescription("Installs and restores packages which are specified in packages.config")]
        [ArgShortcut("i")]
        public string Install { get; set; }

        [ArgShortcut("g")]
        [ArgDescription("Installs and restores global packages which are specified in packages.config")]
        public bool Global { get; set; }

        [ArgDescription("Creates a packages.config file based on the packages directory")]
        public bool Save { get; set; }

        [ArgDescription("Cleans installed packages from working directory")]
        public bool Clean { get; set; }

        [ArgShortcut("pre")]
        [ArgDescription("Allows installation of packages' prelease versions")]
        public bool AllowPreRelease { get; set; }

        [ArgDescription("Outputs version information")]
        public bool Version { get; set; }

        [ArgDescription("Watch the script file and reload it when changed")]
        public bool Watch { get; set; }

        [ArgDescription("Specify modules to load")]
        public string Modules { get; set; }

        [ArgDescription("Defines config file name")]
        public string Config { get; set; }

        [ArgDescription("Defines the version of the package to install. Used in conjunction with -install")]
        public string PackageVersion { get; set; }

        [ArgDescription("Write all console output to the specified file")]
        public string Output { get; set; }

        public static ScriptCsArgs Parse(string[] args)
        {
            Guard.AgainstNullArgument("args", args);

            var curatedArgs = new List<string>();
            string implicitPackageVersion = null;
            for (var index = 0; index < args.Length; ++index)
            {
                if (index < args.Length - 2 &&
                    (string.Equals(args[index], "-install", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(args[index], "-i", StringComparison.OrdinalIgnoreCase)) &&
                    !args[index + 1].StartsWith("-", StringComparison.Ordinal) &&
                    !args[index + 2].StartsWith("-", StringComparison.Ordinal))
                {
                    curatedArgs.Add(args[index]);
                    curatedArgs.Add(args[index + 1]);
                    implicitPackageVersion = args[index + 2];
                    index += 2;
                }
                else
                {
                    curatedArgs.Add(args[index]);
                }
            }

            var scriptCsArgs = Args.Parse<ScriptCsArgs>(curatedArgs.ToArray());
            scriptCsArgs.PackageVersion = scriptCsArgs.PackageVersion ?? implicitPackageVersion;
            return scriptCsArgs;
        }

        public static string GetUsage()
        {
            return ArgUsage.GetUsage<ScriptCsArgs>(
                null, new ArgUsageOptions { ShowPosition = false, ShowType = false, });
        }
    }
}
