using System.IO;
using ScriptCs.Contracts;
using ScriptCs.Hosting;

namespace ScriptCs
{
    public static class ScriptServicesBuilderFactory
    {
        public static IScriptServicesBuilder Create(Config config, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("commandArgs", config);
            Guard.AgainstNullArgument("scriptArgs", scriptArgs);

            IConsole console = new ScriptConsole();
            if (!string.IsNullOrWhiteSpace(config.OutputFile))
            {
                console = new FileConsole(config.OutputFile, console);
            }

            var logProvider = new ColoredConsoleLogProvider(config.LogLevel, console);
            var initializationServices = new InitializationServices(logProvider);
            initializationServices.GetAppDomainAssemblyResolver().Initialize();

            // NOTE (adamralph): this is a hideous assumption about what happens inside the CommandFactory.
            // It is a result of the ScriptServicesBuilderFactory also having to know what is going to happen inside the
            // Command Factory so that it builds the builder(:-p) correctly in advance.
            // This demonstrates the technical debt that exists with the ScriptServicesBuilderFactory and CommandFactory
            // in their current form. We have a separate refactoring task raised to address this.
            var repl = config.Repl ||
                (!config.Clean && config.PackageName == null && !config.Save && config.ScriptName == null);

            var scriptServicesBuilder = new ScriptServicesBuilder(console, logProvider, null, null, initializationServices)
                .Cache(config.Cache)
                .Debug(config.Debug)
                .LogLevel(config.LogLevel)
                .ScriptName(config.ScriptName)
                .Repl(repl);

            return scriptServicesBuilder.LoadModules(Path.GetExtension(config.ScriptName) ?? ".csx", config.Modules);
        }
    }
}
