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
        private bool _shouldInitDrirectoryCatalog;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;
        private readonly string[] _args;

        public CompositionRoot(string[] args)
        {
            Guard.AgainstNullArgument("args", args);

            _args = args;
        }

        public void Initialize()
        {
            // Hack to resolve assemblies for MEF catalog before building Autofac container
            var fileSystem = new FileSystem();
            var argumentParser = new ArgumentParser();
            var configParser = new JsonConfigFileParser();
            var argumentHaldler = new ArgumentHandler(argumentParser, configParser, fileSystem);

            var parserResult = argumentHaldler.Parse(_args);

            var debug = parserResult.CommandArguments.Debug;
            var logLevel = parserResult.CommandArguments.LogLevel;
            _shouldInitDrirectoryCatalog = ShouldInitDrirectoryCatalog(parserResult.CommandArguments);

            var builder = new ContainerBuilder();

            var loggerConfigurator = new LoggerConfigurator(logLevel);
            loggerConfigurator.Configure();
            var logger = loggerConfigurator.GetLogger();

            builder.RegisterInstance<ILog>(logger).Exported(x => x.As<ILog>());
            builder.RegisterType<ReplConsole>().As<IConsole>().Exported(x => x.As<IConsole>());

            builder.RegisterInstance<IArgumentParser>(argumentParser).Exported(x => x.As<IArgumentParser>());
            builder.RegisterInstance<IConfigFileParser>(configParser).Exported(x => x.As<IConfigFileParser>());
            builder.RegisterInstance<IArgumentHandler>(argumentHaldler).Exported(x => x.As<IArgumentHandler>());

            var types = new[]
                {
                    typeof (ScriptHostFactory),
                    typeof (FilePreProcessor),
                    typeof (ScriptPackResolver),
                    typeof (NugetInstallationProvider),
                    typeof (PackageInstaller),
                };

            builder.RegisterTypes(types).AsImplementedInterfaces();
            
            if (debug)
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
            var packageContainer = new PackageContainer(fileSystem);
            var packageAssemblyResolver = new PackageAssemblyResolver(fileSystem, packageContainer);
            var assemblyResolver = new AssemblyResolver(fileSystem, packageAssemblyResolver, assemblyUtility, logger);

            builder.RegisterInstance(fileSystem).As<IFileSystem>();
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

        private static bool ShouldInitDrirectoryCatalog(ScriptCsArgs args)
        {
            return args.Repl || !string.IsNullOrWhiteSpace(args.ScriptName);
        }
    }
}
