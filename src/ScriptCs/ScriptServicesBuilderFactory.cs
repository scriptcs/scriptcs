using System;
using System.IO;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using ScriptCs.Logging;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs
{
    public static class ScriptServicesBuilderFactory
    {
        public static IScriptServicesBuilder Create(Config config, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("commandArgs", config);
            Guard.AgainstNullArgument("scriptArgs", scriptArgs);

            IConsole console = new ScriptConsole();
            if (!string.IsNullOrWhiteSpace(config.Output))
            {
                console = new FileConsole(config.Output, console);
            }

            var configurator = new LoggerConfigurator(config.LogLevel);
            configurator.Configure(console, new NoOpLogger());
            var logger = configurator.GetLogger();
            var initializationServices = new InitializationServices(logger);
            initializationServices.GetAppDomainAssemblyResolver().Initialize();

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger, null, null, initializationServices)
                .Cache(config.Cache)
                .Debug(config.Debug)
                .LogLevel(config.LogLevel)
                .ScriptName(config.ScriptName)
                .Repl(config.Repl);

            var modules = config.Modules == null
                ? new string[0]
                : config.Modules.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            return scriptServicesBuilder.LoadModules(Path.GetExtension(config.ScriptName) ?? ".csx", modules);
        }

        private class NoOpLogger : ILog
        {
            public bool Log(
                Logging.LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                return false;
            }
        }
    }
}
