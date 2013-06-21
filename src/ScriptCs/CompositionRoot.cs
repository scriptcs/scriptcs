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
        private bool _debug;
        private LogLevel _logLevel; 
        private bool _shouldInitDrirectoryCatalog;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;
        private readonly string[] _args;
        private readonly ArgumentParser _argumentParser;
        private readonly IFileSystem _fileSystem;

        public CompositionRoot(string[] args)
        {
            Guard.AgainstNullArgument("args", args);

            _args = args;

            // Hack to resolve assemblies for MEF catalog before building Autofac container
            _fileSystem = new FileSystem();
            _argumentParser = new ArgumentParser(_args, _fileSystem);
        }

        public void Initialize()
        {
            _debug = _argumentParser.CommandArguments.Debug;
            _logLevel = _argumentParser.CommandArguments.LogLevel;
            _shouldInitDrirectoryCatalog = ShouldInitDrirectoryCatalog(_argumentParser.CommandArguments);

            var builder = new ContainerBuilder();

            var loggerConfigurator = new LoggerConfigurator(_logLevel);
            loggerConfigurator.Configure();
            var logger = loggerConfigurator.GetLogger();

            builder.RegisterInstance<ILog>(logger).Exported(x => x.As<ILog>());
            builder.RegisterType<ReplConsole>().As<IConsole>().Exported(x => x.As<IConsole>());

            var types = new[]
                {
                    typeof (ScriptHostFactory),
                    typeof (FilePreProcessor),
                    typeof (ScriptPackResolver),
                    typeof (NugetInstallationProvider),
                    typeof (PackageInstaller),
                };

            builder.RegisterTypes(types).AsImplementedInterfaces();
            
            if (_debug)
            {
                builder.RegisterType<DebugScriptExecutor>().As<IScriptExecutor>();
                builder.RegisterType<RoslynScriptDebuggerEngine>().As<IScriptEngine>();
            }
            else
            {
                builder.RegisterType<ScriptExecutor>().As<IScriptExecutor>();
                builder.RegisterType<RoslynScriptEngine>().As<IScriptEngine>();
            }

            builder.RegisterType<ScriptServiceRoot>().As<ScriptServiceRoot>();

            var assemblyUtility = new AssemblyUtility();
            var packageContainer = new PackageContainer(_fileSystem);
            var packageAssemblyResolver = new PackageAssemblyResolver(_fileSystem, packageContainer);
            var assemblyResolver = new AssemblyResolver(_fileSystem, packageAssemblyResolver, assemblyUtility, logger);

            builder.RegisterInstance(_fileSystem).As<IFileSystem>();
            builder.RegisterInstance(assemblyUtility).As<IAssemblyUtility>();
            builder.RegisterInstance(packageContainer).As<IPackageContainer>();
            builder.RegisterInstance(packageAssemblyResolver).As<IPackageAssemblyResolver>();
            builder.RegisterInstance(assemblyResolver).As<IAssemblyResolver>();

            if (_shouldInitDrirectoryCatalog)
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

        public ArgumentParser GetArgumentParser()
        {
            return _argumentParser;
        }

        private static bool ShouldInitDrirectoryCatalog(ScriptCsArgs args)
        {
            return args.Repl || !string.IsNullOrWhiteSpace(args.ScriptName);
        }
    }
}
