using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptServicesFactory
    {
        private readonly ScriptCsArgs _args;

        public ScriptServicesFactory(ScriptCsArgs args)
        {
            _args = args;
        }

        public ScriptServices Create(IConsole console = null)
        {
            if (console == null)
            {
                console = new ScriptConsole();
            }

            var configurator = new LoggerConfigurator(_args.LogLevel);
            configurator.Configure(console);
            var logger = configurator.GetLogger();

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger)
                .Cache(_args.Cache)
                .LogLevel(_args.LogLevel)
                .ScriptName(_args.ScriptName)
                .Repl(_args.Repl);

            var modules = GetModuleList(_args.Modules);
            var extension = Path.GetExtension(_args.ScriptName);

            if (string.IsNullOrWhiteSpace(extension) && !_args.Repl)
            {
                // No extension was given, i.e we might have something like
                // "scriptcs foo" to deal with. We activate the default extension,
                // to make sure it's given to the LoadModules below.
                extension = ".csx";

                if (!string.IsNullOrWhiteSpace(_args.ScriptName))
                {
                    // If the was in fact a script specified, we'll extend it
                    // with the default extension, assuming the user giving
                    // "scriptcs foo" actually meant "scriptcs foo.csx". We
                    // perform no validation here thought; let it be done by
                    // the activated command. If the file don't exist, it's
                    // up to the command to detect and report.

                    _args.ScriptName += extension;
                }
            }

            scriptServicesBuilder.LoadModules(extension, modules);
            var scriptServiceRoot = scriptServicesBuilder.Build();

            return scriptServiceRoot;
        }

        private static string[] GetModuleList(string modulesArg)
        {
            var modules = new string[0];
            if (modulesArg != null)
            {
                modules = modulesArg.Split(',');
            }
            return modules;
        }
    }
}