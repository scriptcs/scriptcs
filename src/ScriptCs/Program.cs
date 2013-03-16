using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using PowerArgs;
using ScriptCs.Command;
using ScriptCs.Engine.Roslyn;
using System.ComponentModel.Composition.Registration;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var commandArgs = Args.Parse<ScriptCsArgs>(args);

            if (!commandArgs.IsValid())
            {
                Console.WriteLine(ArgUsage.GetUsage<ScriptCsArgs>());
                return;
            }

            var debug = commandArgs.DebugFlag;

            var container = ConfigureMef(debug);
            var commandFactory = new CommandFactory(container);
            var command = commandFactory.CreateCommand(commandArgs);

            command.Execute();
        }

        private static CompositionContainer ConfigureMef(bool debug)
        {
            var conventions = SetupMefConventions(debug);

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly, conventions));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ScriptExecutor).Assembly, conventions));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(RoslynScriptEngine).Assembly, conventions));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.pack.dll", conventions));
            return new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);
        }

        private static RegistrationBuilder SetupMefConventions(bool debug)
        {
            var conventions = new RegistrationBuilder();

            conventions.ForTypesDerivedFrom<IScriptHostFactory>().Export<IScriptHostFactory>();
            conventions.ForTypesDerivedFrom<IFileSystem>().Export<IFileSystem>();
            conventions.ForTypesDerivedFrom<IPackageAssemblyResolver>().Export<IPackageAssemblyResolver>();
            conventions.ForTypesDerivedFrom<IPackageContainer>().Export<IPackageContainer>();
            conventions.ForTypesDerivedFrom<IScriptPack>().Export<IScriptPack>();
            conventions.ForTypesDerivedFrom<IFilePreProcessor>().Export<IFilePreProcessor>();
            conventions.ForTypesDerivedFrom<IPackageInstaller>().Export<IPackageInstaller>();
            conventions.ForTypesDerivedFrom<IInstallationProvider>().Export<IInstallationProvider>();

            if (debug)
            {
                conventions.ForType<DebugScriptExecutor>().Export<IScriptExecutor>();
                conventions.ForType<RoslynScriptDebuggerEngine>().Export<IScriptEngine>();
            }
            else
            {
                conventions.ForType<ScriptExecutor>().Export<IScriptExecutor>();
                conventions.ForType<RoslynScriptEngine>().Export<IScriptEngine>();
            }

            return conventions;
        }
    }
}