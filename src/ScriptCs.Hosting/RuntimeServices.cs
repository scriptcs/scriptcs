using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mef;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;

namespace ScriptCs.Hosting
{
    public class RuntimeServices : ScriptServicesRegistration, IRuntimeServices
    {
        private readonly ILog _log;
        private readonly IConsole _console;
        private readonly Type _scriptEngineType;
        private readonly Type _scriptExecutorType;
        private readonly Type _replType;
        private readonly bool _initDirectoryCatalog;
        private readonly IInitializationServices _initializationServices;
        private readonly string _scriptName;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public RuntimeServices(
            Common.Logging.ILog logger,
            IDictionary<Type, object> overrides,
            IConsole console,
            Type scriptEngineType,
            Type scriptExecutorType,
            Type replType,
            bool initDirectoryCatalog,
            IInitializationServices initializationServices,
            string scriptName)
            : this(
                new CommonLoggingLogProvider(logger),
                overrides,
                console,
                scriptEngineType,
                scriptExecutorType,
                replType,
                initDirectoryCatalog,
                initializationServices,
                scriptName)
        {
        }

        public RuntimeServices(
            ILogProvider logProvider,
            IDictionary<Type, object> overrides,
            IConsole console,
            Type scriptEngineType,
            Type scriptExecutorType,
            Type replType,
            bool initDirectoryCatalog,
            IInitializationServices initializationServices,
            string scriptName)
            : base(logProvider, overrides)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _log = logProvider.ForCurrentType();
            _console = console;
            _scriptEngineType = scriptEngineType;
            _scriptExecutorType = scriptExecutorType;
            _replType = replType;
            _initDirectoryCatalog = initDirectoryCatalog;
            _initializationServices = initializationServices;
            _scriptName = scriptName;
        }

        internal bool InitDirectoryCatalog
        {
            get { return _initDirectoryCatalog; }
        }

        protected override IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            _log.Debug("Registering runtime services");

            builder.RegisterInstance(this.LogProvider).Exported(x => x.As<ILogProvider>());
            builder.RegisterType(_scriptEngineType).As<IScriptEngine>().SingleInstance();
            builder.RegisterType(_scriptExecutorType).As<IScriptExecutor>().SingleInstance();
            builder.RegisterType(_replType).As<IRepl>().SingleInstance();
            builder.RegisterType<ScriptServices>().SingleInstance();
            builder.RegisterType<Repl>().As<IRepl>().SingleInstance();
            builder.RegisterType<Printers>().SingleInstance();

            RegisterLineProcessors(builder);
            RegisterReplCommands(builder);

            RegisterOverrideOrDefault<IFileSystem>(
                builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());

            RegisterOverrideOrDefault<IAssemblyUtility>(
                builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());

            RegisterOverrideOrDefault<IPackageContainer>(
                builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>().SingleInstance());

            RegisterOverrideOrDefault<IPackageAssemblyResolver>(
                builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());

            RegisterOverrideOrDefault<IAssemblyResolver>(
                builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>().SingleInstance());

            RegisterOverrideOrDefault<IScriptHostFactory>(
                builder, b => b.RegisterType<ScriptHostFactory>().As<IScriptHostFactory>().SingleInstance());

            RegisterOverrideOrDefault<IFilePreProcessor>(
                builder, b => b.RegisterType<FilePreProcessor>().As<IFilePreProcessor>().SingleInstance());

            RegisterOverrideOrDefault<IScriptPackResolver>(
                builder, b => b.RegisterType<ScriptPackResolver>().As<IScriptPackResolver>().SingleInstance());

            RegisterOverrideOrDefault<IInstallationProvider>(
                builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>().SingleInstance());

            RegisterOverrideOrDefault<IPackageInstaller>(
                builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>().SingleInstance());

            RegisterOverrideOrDefault<ScriptServices>(
                builder, b => b.RegisterType<ScriptServices>().SingleInstance());

            RegisterOverrideOrDefault<IObjectSerializer>(
                builder, b => b.RegisterType<ObjectSerializer>().As<IObjectSerializer>().SingleInstance());

            RegisterOverrideOrDefault<IConsole>(
                builder, b => b.RegisterInstance(_console));

            RegisterOverrideOrDefault<IFileSystemMigrator>(
                builder, b => b.RegisterType<FileSystemMigrator>().As<IFileSystemMigrator>().SingleInstance());

            RegisterOverrideOrDefault<IScriptLibraryComposer>(
                builder, b => b.RegisterType<ScriptLibraryComposer>().As<IScriptLibraryComposer>().SingleInstance());

            RegisterOverrideOrDefault<IVisualStudioSolutionWriter>(
                builder, b => b.RegisterType<VisualStudioSolutionWriter>().As<IVisualStudioSolutionWriter>().SingleInstance());

            if (_initDirectoryCatalog)
            {
                var fileSystem = _initializationServices.GetFileSystem();

                var assemblies = _initializationServices.GetAssemblyResolver()
                    .GetAssemblyPaths(fileSystem.GetWorkingDirectory(_scriptName))
                    .Where(assembly => ShouldLoadAssembly(fileSystem, _initializationServices.GetAssemblyUtility(), assembly));

                var aggregateCatalog = new AggregateCatalog();
                var assemblyLoadFailures = false;

                foreach (var assemblyPath in assemblies)
                {
                    try
                    {
                        var catalog = new AssemblyCatalog(assemblyPath);
                        // force the parts to be queried to catch any errors that would otherwise show up later
                        catalog.Parts.ToList();
                        aggregateCatalog.Catalogs.Add(catalog);
                    }
                    catch (ReflectionTypeLoadException typeLoadEx)
                    {
                        assemblyLoadFailures = true;
                        if (typeLoadEx.LoaderExceptions != null && typeLoadEx.LoaderExceptions.Any())
                        {
                            foreach (var ex in typeLoadEx.LoaderExceptions.GroupBy(x => x.Message))
                            {
                                _log.DebugFormat(
                                    "Failure loading assembly: {0}. Exception: {1}", assemblyPath, ex.First().Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        assemblyLoadFailures = true;
                        _log.DebugFormat("Failure loading assembly: {0}. Exception: {1}", assemblyPath, ex.Message);
                    }
                }
                if (assemblyLoadFailures)
                {
                    _log.Warn(string.IsNullOrEmpty(_scriptName)
                        ? "Some assemblies failed to load. Launch with '-repl -loglevel debug' to see the details"
                        : "Some assemblies failed to load. Launch with '-loglevel debug' to see the details");
                }
                builder.RegisterComposablePartCatalog(aggregateCatalog);
            }

            return builder.Build();
        }

        // HACK: Filter out assemblies in the GAC by checking if full path is specified.
        private static bool ShouldLoadAssembly(IFileSystem fileSystem, IAssemblyUtility assemblyUtility, string assembly)
        {
            return fileSystem.IsPathRooted(assembly) && assemblyUtility.IsManagedAssembly(assembly);
        }

        private static void RegisterReplCommands(ContainerBuilder builder)
        {
            var replCommands = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic)
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => typeof(IReplCommand).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .ToArray();

            builder.RegisterTypes(replCommands).As<IReplCommand>();
        }

        public ScriptServices GetScriptServices()
        {
            _log.Debug("Resolving ScriptServices");
            return Container.Resolve<ScriptServices>();
        }
    }
}
