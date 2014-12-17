using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;

namespace ScriptCs.Hosting
{
    public class RuntimeServices : ScriptServicesRegistration, IRuntimeServices
    {
        private readonly IConsole _console;
        private readonly Type _scriptEngineType;
        private readonly Type _scriptExecutorType;
        private readonly bool _initDirectoryCatalog;
        private readonly IInitializationServices _initializationServices;
        private readonly string _scriptName;

        public RuntimeServices(ILog logger, IDictionary<Type, object> overrides, IConsole console, Type scriptEngineType, Type scriptExecutorType, bool initDirectoryCatalog, IInitializationServices initializationServices, string scriptName) :
            base(logger, overrides)
        {
            _console = console;
            _scriptEngineType = scriptEngineType;
            _scriptExecutorType = scriptExecutorType;
            _initDirectoryCatalog = initDirectoryCatalog;
            _initializationServices = initializationServices;
            _scriptName = scriptName;
        }

        protected override IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            this.Logger.Debug("Registering runtime services");

            builder.RegisterInstance<ILog>(this.Logger).Exported(x => x.As<ILog>());
            builder.RegisterType(_scriptEngineType).As<IScriptEngine>().SingleInstance();
            builder.RegisterType(_scriptExecutorType).As<IScriptExecutor>().SingleInstance();
            builder.RegisterType<ScriptServices>().SingleInstance();
            builder.RegisterType<Repl>().As<IRepl>().SingleInstance();

            RegisterLineProcessors(builder);
            RegisterReplCommands(builder);

            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>().SingleInstance());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IScriptHostFactory>(builder, b => b.RegisterType<ScriptHostFactory>().As<IScriptHostFactory>().SingleInstance());
            RegisterOverrideOrDefault<IFilePreProcessor>(builder, b => b.RegisterType<FilePreProcessor>().As<IFilePreProcessor>().SingleInstance());
            RegisterOverrideOrDefault<IScriptPackResolver>(builder, b => b.RegisterType<ScriptPackResolver>().As<IScriptPackResolver>().SingleInstance());
            RegisterOverrideOrDefault<IInstallationProvider>(builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>().SingleInstance());
            RegisterOverrideOrDefault<IPackageInstaller>(builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>().SingleInstance());
            RegisterOverrideOrDefault<ScriptServices>(builder, b => b.RegisterType<ScriptServices>().SingleInstance());
            RegisterOverrideOrDefault<IObjectSerializer>(builder, b => b.RegisterType<ObjectSerializer>().As<IObjectSerializer>().SingleInstance());
            RegisterOverrideOrDefault<IConsole>(builder, b => b.RegisterInstance(_console));

            var assemblyResolver = _initializationServices.GetAssemblyResolver();

            if (_initDirectoryCatalog)
            {
                var fileSystem = _initializationServices.GetFileSystem();
                var currentDirectory = fileSystem.GetWorkingDirectory(_scriptName);

                var assemblies = assemblyResolver
                    .GetAssemblyPaths(currentDirectory)
                    .Where(assembly => ShouldLoadAssembly(fileSystem, assembly));

                var aggregateCatalog = new AggregateCatalog();
                bool assemblyLoadFailures = false;

                foreach (var assemblyPath in assemblies)
                {
                    try
                    {
                        var catalog = new AssemblyCatalog(assemblyPath);
                        //force the parts to be queried to catch any errors that will shwo up later
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
                                Logger.DebugFormat("Failure loading assembly: {0}. Exception: {1}", assemblyPath,
                                    ex.First().Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        assemblyLoadFailures = true;
                        Logger.DebugFormat("Failure loading assembly: {0}. Exception: {1}", assemblyPath, ex.Message);
                    }
                }
                if (assemblyLoadFailures)
                {
                    Logger.Warn(string.IsNullOrEmpty(_scriptName)
                        ? "Some assemblies failed to load. Launch with '-repl -loglevel debug' to see the details"
                        : "Some assemblies failed to load. Launch with '-loglevel debug' to see the details");
                }
                builder.RegisterComposablePartCatalog(aggregateCatalog);
            }

            return builder.Build();
        }

        // HACK: Filter out assemblies in the GAC by checking if full path is specified.
        private static bool ShouldLoadAssembly(IFileSystem fileSystem, string assembly)
        {
            return fileSystem.IsPathRooted(assembly);
        }

        private void RegisterLineProcessors(ContainerBuilder builder)
        {
            object processors;
            this.Overrides.TryGetValue(typeof(ILineProcessor), out processors);
            var processorList = (processors as IEnumerable<Type> ?? Enumerable.Empty<Type>()).ToArray();

            var loadProcessorType = processorList
                .FirstOrDefault(x => typeof(ILoadLineProcessor).IsAssignableFrom(x))
                ?? typeof(LoadLineProcessor);

            var usingProcessorType = processorList
                .FirstOrDefault(x => typeof(IUsingLineProcessor).IsAssignableFrom(x))
                ?? typeof(UsingLineProcessor);

            var referenceProcessorType = processorList
                .FirstOrDefault(x => typeof(IReferenceLineProcessor).IsAssignableFrom(x))
                ?? typeof(ReferenceLineProcessor);

            var processorArray = new[] { loadProcessorType, usingProcessorType, referenceProcessorType }.Union(processorList).ToArray();

            builder.RegisterTypes(processorArray).As<ILineProcessor>();
        }

        private void RegisterReplCommands(ContainerBuilder builder)
        {
            var replCommands = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).SelectMany(x => x.GetExportedTypes()).Where(x => typeof(IReplCommand).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);
            builder.RegisterTypes(replCommands.ToArray()).As<IReplCommand>();
        }

        public ScriptServices GetScriptServices()
        {
            this.Logger.Debug("Resolving ScriptServices");
            return Container.Resolve<ScriptServices>();
        }
    }
}
