using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

using Autofac;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class CompositionRoot
    {
        private readonly LogLevel _logLevel; 
        private readonly bool _shouldInitDirectoryCatalog;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;
        private IDictionary<Type, object> _overrides;
        private Type _scriptExecutorType;
        private Type _scriptEngineType;

        public CompositionRoot(ScriptCsArgs args, Type scriptExecutorType, Type scriptEngineType) : this(args, scriptExecutorType, scriptEngineType, new Dictionary<Type, object>())
        {
        }

        public CompositionRoot(ScriptCsArgs args, Type scriptExecutorType, Type scriptEngineType, IDictionary<Type, Object> overrides)
        {
            Guard.AgainstNullArgument("args", args);
            Guard.AgainstNullArgument("scriptExecutor", scriptExecutorType);
            Guard.AgainstNullArgument("scriptEngine", scriptEngineType);

            if (!typeof(IScriptExecutor).IsAssignableFrom(scriptExecutorType))
            {
                throw new ArgumentException("scriptExecutor type must implement IScriptExecutor", "scriptExecutor");
            }
            
            if (!typeof (IScriptEngine).IsAssignableFrom(scriptEngineType))
            {
                throw new ArgumentException("scriptEngine type must implement IScriptEngine", "scriptEngine");
            }

            _scriptExecutorType = scriptExecutorType;
            _scriptEngineType = scriptEngineType;
            
            _overrides = overrides;
            _logLevel = args.LogLevel;
            _shouldInitDirectoryCatalog = ShouldInitDirectoryCatalog(args);
        }

        public void RegisterOverrideOrDefault<T>(ContainerBuilder builder, Action<ContainerBuilder> registrationAction)
        {
            if (_overrides.ContainsKey(typeof (T)))
            {
                var reg = _overrides[typeof(T)];
                if (reg.GetType().IsSubclassOf(typeof (Type)))
                {
                    builder.RegisterType((Type) reg).As<T>();
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

        public void Initialize()
        {            
            var builder = new ContainerBuilder();
            builder.RegisterType<ReplConsole>().As<IConsole>().Exported(x => x.As<IConsole>());

            var loggerConfigurator = new LoggerConfigurator(_logLevel);
            loggerConfigurator.Configure(new ReplConsole());
            var logger = loggerConfigurator.GetLogger();
           
            builder.RegisterInstance<ILog>(logger).Exported(x => x.As<ILog>());
            builder.RegisterType(_scriptEngineType).As<IScriptEngine>();
            builder.RegisterType(_scriptExecutorType).As<IScriptExecutor>();

            RegisterOverrideOrDefault<IScriptHostFactory>(builder, b => b.RegisterType<ScriptHostFactory>().As<IScriptHostFactory>());
            RegisterOverrideOrDefault<IFilePreProcessor>(builder, b => b.RegisterType<FilePreProcessor>().As<IFilePreProcessor>());
            RegisterOverrideOrDefault<IScriptPackResolver>(builder, b => b.RegisterType<ScriptPackResolver>().As<IScriptPackResolver>());
            RegisterOverrideOrDefault<IInstallationProvider>(builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>());
            RegisterOverrideOrDefault<IPackageInstaller>(builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>());
            RegisterOverrideOrDefault<ScriptServiceRoot>(builder, b => b.RegisterType<ScriptServiceRoot>());

            // Hack using a second container to resolve assemblies for MEF catalog before building Autofac container
            var tempBuilder = new ContainerBuilder();

            tempBuilder.RegisterInstance<ILog>(logger);
            RegisterOverrideOrDefault<IFileSystem>(tempBuilder, b => b.RegisterType<FileSystem>().As<IFileSystem>());
            RegisterOverrideOrDefault<IAssemblyUtility>(tempBuilder, b=>b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>());
            RegisterOverrideOrDefault<IPackageContainer>(tempBuilder, b=>b.RegisterType<PackageContainer>().As<IPackageContainer>());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(tempBuilder, b=>b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>());
            RegisterOverrideOrDefault<IAssemblyResolver>(tempBuilder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>());

            var tempContainer = tempBuilder.Build();
            var assemblyResolver = tempContainer.Resolve<IAssemblyResolver>();

            builder.RegisterInstance(tempContainer.Resolve<IFileSystem>()).As<IFileSystem>();
            builder.RegisterInstance(tempContainer.Resolve<IAssemblyUtility>()).As<IAssemblyUtility>();
            builder.RegisterInstance(tempContainer.Resolve<IPackageContainer>()).As<IPackageContainer>();
            builder.RegisterInstance(tempContainer.Resolve<IPackageAssemblyResolver>()).As<IPackageAssemblyResolver>();
            builder.RegisterInstance(assemblyResolver).As<IAssemblyResolver>();

            if (_shouldInitDirectoryCatalog)
            {
                var currentDirectory = Environment.CurrentDirectory;
                var assemblies = assemblyResolver.GetAssemblyPaths(currentDirectory);

                var aggregateCatalog = new AggregateCatalog();

                assemblies.Select(x => new AssemblyCatalog(x)).ToList()
                    .ForEach(catalog => aggregateCatalog.Catalogs.Add(catalog));

                builder.RegisterComposablePartCatalog(aggregateCatalog);
            }

            _container = builder.Build();

            _scriptServiceRoot = _container.Resolve<ScriptServiceRoot>();            
        }

        public ScriptServiceRoot GetServiceRoot()
        {
            return _scriptServiceRoot;
        }

        public ILog GetLogger()
        {
            return _container.Resolve<ILog>();
        }

        private static bool ShouldInitDirectoryCatalog(ScriptCsArgs args)
        {
            return args.Repl || !string.IsNullOrWhiteSpace(args.ScriptName);
        }
    }
}
