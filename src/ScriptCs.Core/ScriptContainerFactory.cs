using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class ScriptContainerFactory : IScriptContainerFactory
    {
        private readonly ILog _logger;
        private readonly IConsole _console;
        private readonly Type _scriptEngineType;
        private readonly Type _scriptExecutorType;
        private readonly bool _initDirectoryCatalog;
        private IDictionary<Type, object> _overrides = null;

        public ScriptContainerFactory(ILog logger, IConsole console, Type scriptEngineType, Type scriptExecutorType, bool initDirectoryCatalog, IDictionary<Type, object> overrides )
        {
            Guard.AgainstNullArgument("scriptExecutor", scriptExecutorType);
            Guard.AgainstNullArgument("scriptEngine", scriptEngineType);
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("logger", logger);

            if (overrides == null)
                overrides = new Dictionary<Type, object>();

            _logger = logger;
            _console = console;
            _scriptEngineType = scriptEngineType;
            _scriptExecutorType = scriptExecutorType;
            _initDirectoryCatalog = initDirectoryCatalog;
            _overrides = overrides;
        }

        private IContainer _initializationContainer;
        public IContainer InitializationContainer
        {
            get
            {
                if (_initializationContainer == null)
                {
                    _initializationContainer = CreateInitializationContainer();
                }
                return _initializationContainer;
            }
        }

        private IContainer _runtimeContainer;
        public IContainer RuntimeContainer
        {
            get
            {
                if (_runtimeContainer == null)
                {
                    _runtimeContainer = CreateRuntimeContainer();
                }
                return _runtimeContainer;
            }
        }

        private IContainer CreateInitializationContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILog>(_logger);
            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>());
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>());
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>());
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>());
            return builder.Build();
        }

        private IContainer CreateRuntimeContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILog>(_logger).Exported(x => x.As<ILog>());
            builder.RegisterType(_scriptEngineType).As<IScriptEngine>();
            builder.RegisterType(_scriptExecutorType).As<IScriptExecutor>();
            builder.RegisterInstance(_console).As<IConsole>();
            builder.RegisterType<ScriptServices>();

            RegisterOverrideOrDefault<IScriptHostFactory>(builder, b => b.RegisterType<ScriptHostFactory>().As<IScriptHostFactory>());
            RegisterOverrideOrDefault<IFilePreProcessor>(builder, b => b.RegisterType<FilePreProcessor>().As<IFilePreProcessor>());
            RegisterOverrideOrDefault<IScriptPackResolver>(builder, b => b.RegisterType<ScriptPackResolver>().As<IScriptPackResolver>());
            RegisterOverrideOrDefault<IInstallationProvider>(builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>());
            RegisterOverrideOrDefault<IPackageInstaller>(builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>());
            RegisterOverrideOrDefault<ScriptServices>(builder, b => b.RegisterType<ScriptServices>());

            var initializationContainer = InitializationContainer;
            var assemblyResolver = initializationContainer.Resolve<IAssemblyResolver>();

            builder.RegisterInstance(initializationContainer.Resolve<IFileSystem>()).As<IFileSystem>();
            builder.RegisterInstance(initializationContainer.Resolve<IAssemblyUtility>()).As<IAssemblyUtility>();
            builder.RegisterInstance(initializationContainer.Resolve<IPackageContainer>()).As<IPackageContainer>();
            builder.RegisterInstance(initializationContainer.Resolve<IPackageAssemblyResolver>()).As<IPackageAssemblyResolver>();
            builder.RegisterInstance(assemblyResolver).As<IAssemblyResolver>();

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

        private void RegisterOverrideOrDefault<T>(ContainerBuilder builder, Action<ContainerBuilder> registrationAction)
        {
            if (_overrides.ContainsKey(typeof(T)))
            {
                var reg = _overrides[typeof(T)];
                if (reg.GetType().IsSubclassOf(typeof(Type)))
                {
                    builder.RegisterType((Type)reg).As<T>().SingleInstance();
                }
                else
                {
                    builder.RegisterInstance(reg).As<T>();
                }
            }
            else
            {
                registrationAction(builder);
            }
        }
    }
}
