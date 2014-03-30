﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Autofac;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;

namespace ScriptCs.Hosting
{
    public class RuntimeServices : ScriptServicesRegistration, IRuntimeServices
    {
        private readonly IList<Type> _lineProcessors;
        private readonly IList<Type> _codeRewriters;
        private readonly IConsole _console;
        private readonly Type _scriptEngineType;
        private readonly Type _scriptExecutorType;
        private readonly bool _initDirectoryCatalog;
        private readonly IInitializationServices _initializationServices;
        private readonly string _scriptName;

        public RuntimeServices(ILog logger, IDictionary<Type, object> overrides, IList<Type> lineProcessors, IList<Type> codeRewriters, IConsole console, Type scriptEngineType, Type scriptExecutorType, bool initDirectoryCatalog, IInitializationServices initializationServices, string scriptName) :
            base(logger, overrides)
        {
            _lineProcessors = lineProcessors;
            _codeRewriters = codeRewriters;
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
            Logger.Debug("Registering runtime services");

            builder.RegisterInstance<ILog>(this.Logger).Exported(x => x.As<ILog>());
            builder.RegisterType(_scriptEngineType).As<IScriptEngine>().SingleInstance();
            builder.RegisterType(_scriptExecutorType).As<IScriptExecutor>().SingleInstance();
            builder.RegisterType<ScriptServices>().SingleInstance();

            builder.RegisterTypes(_codeRewriters.ToArray()).As<ICodeRewriter>();
            RegisterLineProcessors(builder);

            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>().SingleInstance());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IScriptHostFactory>(builder, b => b.RegisterType<ScriptHostFactory>().As<IScriptHostFactory>().SingleInstance());
            RegisterOverrideOrDefault<IFilePreProcessor>(builder, b => b.RegisterType<RewritingFilePreProcessor>().As<IFilePreProcessor>().SingleInstance());
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
                var assemblies = assemblyResolver.GetAssemblyPaths(currentDirectory);

                var aggregateCatalog = new AggregateCatalog();
                bool assemblyLoadFailures = false;

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var catalog = new AssemblyCatalog(assembly);
                        catalog.Parts.ToList(); //force the Parts to be read
                        aggregateCatalog.Catalogs.Add(catalog);
                    }
                    catch (Exception ex)
                    {
                        assemblyLoadFailures = true;
                        Logger.DebugFormat("Failure loading assembly: {0}. Exception: {1}", assembly, ex.Message);
                    }
                }
                if (assemblyLoadFailures)
                {
                    if (_scriptName == null || _scriptName.Length == 0)
                        Logger.Warn("Some assemblies failed to load. Launch with '-repl -loglevel debug' to see the details");
                    else
                        Logger.Warn("Some assemblies failed to load. Launch with '-loglevel debug' to see the details");
                }
                builder.RegisterComposablePartCatalog(aggregateCatalog);
            }

            return builder.Build();
        }

        private void RegisterLineProcessors(ContainerBuilder builder)
        {
            var loadProcessorType = _lineProcessors
                .FirstOrDefault(x => typeof(ILoadLineProcessor).IsAssignableFrom(x))
                ?? typeof(LoadLineProcessor);

            var usingProcessorType = _lineProcessors
                .FirstOrDefault(x => typeof(IUsingLineProcessor).IsAssignableFrom(x))
                ?? typeof(UsingLineProcessor);

            var referenceProcessorType = _lineProcessors
                .FirstOrDefault(x => typeof(IReferenceLineProcessor).IsAssignableFrom(x))
                ?? typeof(ReferenceLineProcessor);

            var lineProcessors = new[] { loadProcessorType, usingProcessorType, referenceProcessorType }.Union(_lineProcessors);

            builder.RegisterTypes(lineProcessors.ToArray()).As<ILineProcessor>();
        }

        public ScriptServices GetScriptServices()
        {
            this.Logger.Debug("Resolving ScriptServices");
            return Container.Resolve<ScriptServices>();
        }
    }
}
