using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class RuntimeServices : ScriptContainerFactory, IRuntimeServices
    {
        private readonly IConsole _console;
        private readonly Type _scriptEngineType;
        private readonly Type _scriptExecutorType;
        private readonly bool _initDirectoryCatalog;
        private readonly IInitializationServices _initializationServices;

        public RuntimeServices(ILog logger, IDictionary<Type, object> overrides, IConsole console, Type scriptEngineType, Type scriptExecutorType, bool initDirectoryCatalog, IInitializationServices initializationServices) : 
            base(logger, overrides)
        {
            _console = console;
            _scriptEngineType = scriptEngineType;
            _scriptExecutorType = scriptExecutorType;
            _initDirectoryCatalog = initDirectoryCatalog;
            _initializationServices = initializationServices;
        }

        protected override IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            _logger.Debug("Registering runtime services");

            builder.RegisterInstance<ILog>(_logger).Exported(x => x.As<ILog>());
            builder.RegisterType(_scriptEngineType).As<IScriptEngine>().SingleInstance();
            builder.RegisterType(_scriptExecutorType).As<IScriptExecutor>().SingleInstance();
            builder.RegisterInstance(_console).As<IConsole>();
            builder.RegisterType<ScriptServices>().SingleInstance();

            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>().SingleInstance());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<ModuleLoader>(builder, b => b.RegisterType<ModuleLoader>().As<IModuleLoader>().SingleInstance());
            RegisterOverrideOrDefault<IScriptHostFactory>(builder, b => b.RegisterType<ScriptHostFactory>().As<IScriptHostFactory>().SingleInstance());
            RegisterOverrideOrDefault<IFilePreProcessor>(builder, b => b.RegisterType<FilePreProcessor>().As<IFilePreProcessor>().SingleInstance());
            RegisterOverrideOrDefault<IScriptPackResolver>(builder, b => b.RegisterType<ScriptPackResolver>().As<IScriptPackResolver>().SingleInstance());
            RegisterOverrideOrDefault<IInstallationProvider>(builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>().SingleInstance());
            RegisterOverrideOrDefault<IPackageInstaller>(builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>().SingleInstance());
            RegisterOverrideOrDefault<ScriptServices>(builder, b => b.RegisterType<ScriptServices>().SingleInstance());

            var assemblyResolver = _initializationServices.GetAssemblyResolver();

            if (_initDirectoryCatalog)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var assemblies = assemblyResolver.GetAssemblyPaths(currentDirectory);

                var aggregateCatalog = new AggregateCatalog();

                assemblies.Select(x => new AssemblyCatalog(x)).ToList()
                    .ForEach(catalog => aggregateCatalog.Catalogs.Add(catalog));

                builder.RegisterComposablePartCatalog(aggregateCatalog);
            }

            return builder.Build();
        }

        public ScriptServices GetScriptServices()
        {
            _logger.Debug("Resolving ScriptServices");
            return Container.Resolve<ScriptServices>();
        }
    }
}
