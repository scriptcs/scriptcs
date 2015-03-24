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

            // NOTE (adamralph): this is a hideous assumption about what happens inside the CommandFactory.
            // It is a result of the ScriptServicesBuilderFactory also having to know what is going to happen inside the
            // Command Factory so that it builds the builder(:-p) correctly in advance.
            // This demonstrates the technical debt that exists with the ScriptServicesBuilderFactory and CommandFactory
            // in their current form. We have a separate refactoring task raised to address this.
            var repl = config.Repl ||
                (!config.Clean && config.Install == null && !config.Save && config.ScriptName == null);

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger, null, null, initializationServices)
                .Cache(config.Cache)
                .Debug(config.Debug)
                .LogLevel(config.LogLevel)
                .ScriptName(config.ScriptName)
                .Repl(repl);

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
