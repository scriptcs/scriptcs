using System;
using System.IO;
using ScriptCs.Contracts;
using ScriptCs.Hosting;

namespace ScriptCs
{
    public static class ScriptServicesBuilderFactory
    {
        public static IScriptServicesBuilder Create(ScriptCsArgs commandArgs, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("commandArgs", commandArgs);
            Guard.AgainstNullArgument("scriptArgs", scriptArgs);

            IConsole console = new ScriptConsole();
            if (!string.IsNullOrWhiteSpace(commandArgs.Output))
            {
                console = new FileConsole(commandArgs.Output, console);
            }

            var logLevel = commandArgs.LogLevel ?? (commandArgs.Debug ? LogLevel.Debug : LogLevel.Info);
            var configurator = new LoggerConfigurator(logLevel);
            configurator.Configure(console);
            var logger = configurator.GetLogger();
            var initializationServices = new InitializationServices(logger);
            initializationServices.GetAppDomainAssemblyResolver().Initialize();

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger, null, null, initializationServices)
                .Cache(commandArgs.Cache)
                .Debug(commandArgs.Debug)
                .LogLevel(logLevel)
                .ScriptName(commandArgs.ScriptName)
                .Repl(commandArgs.Repl);

            var modules = commandArgs.Modules == null
                ? new string[0]
                : commandArgs.Modules.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var extension = Path.GetExtension(commandArgs.ScriptName);

            if (string.IsNullOrWhiteSpace(extension) && !commandArgs.Repl)
            {
                // No extension was given, i.e we might have something like
                // "scriptcs foo" to deal with. We activate the default extension,
                // to make sure it's given to the LoadModules below.
                extension = ".csx";

                if (!string.IsNullOrWhiteSpace(commandArgs.ScriptName))
                {
                    // If the was in fact a script specified, we'll extend it
                    // with the default extension, assuming the user giving
                    // "scriptcs foo" actually meant "scriptcs foo.csx". We
                    // perform no validation here thought; let it be done by
                    // the activated command. If the file don't exist, it's
                    // up to the command to detect and report.

                    commandArgs.ScriptName += extension;
                }
            }

            return scriptServicesBuilder.LoadModules(extension, modules);
        }
    }
}
