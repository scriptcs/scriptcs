using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

using Autofac;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class CompositionRoot
    {
        private readonly bool _debug;
        private readonly LogLevel _logLevel; 
        private readonly bool _shouldInitDrirectoryCatalog;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;

        public CompositionRoot(ScriptCsArgs args)
        {
            Guard.AgainstNullArgument("args", args);

            _debug = args.Debug;
            _logLevel = args.LogLevel;
            _shouldInitDrirectoryCatalog = ShouldInitDrirectoryCatalog(args);
        }

        public void Initialize()
        {
            var builder = new ContainerBuilder();

            var loggerConfigurator = new LoggerConfigurator(_logLevel);
            loggerConfigurator.Configure();
            var logger = loggerConfigurator.GetLogger();

            builder.RegisterInstance<ILog>(logger);

            var types = new[]
                {
                    typeof (ScriptHostFactory),
                    typeof (FilePreProcessor),
                    typeof (ScriptPackResolver),
                    typeof (NugetInstallationProvider),
                    typeof (PackageInstaller),
                    typeof (ReplConsole),
                    typeof (AssemblyResolver),
                    typeof (AssemblyUtility)
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

            // Newing up these manually to get package assemblies
            var fileSystem = new FileSystem();
            var packageContainer = new PackageContainer(fileSystem);
            var packageAssemblyResolver = new PackageAssemblyResolver(fileSystem, packageContainer);

            builder.RegisterInstance(fileSystem).As<IFileSystem>();
            builder.RegisterInstance(packageContainer).As<IPackageContainer>();
            builder.RegisterInstance(packageAssemblyResolver).As<IPackageAssemblyResolver>();

            if (_shouldInitDrirectoryCatalog)
            {
                var currentDirectory = Environment.CurrentDirectory;
                
                var assemblies = packageAssemblyResolver.GetAssemblyNames(currentDirectory).ToList();

                var binFolder = Path.Combine(currentDirectory, Constants.BinFolder);
                if (Directory.Exists(binFolder))
                {
                    var binAssemblies = Directory.EnumerateFiles(binFolder, "*.dll")
                        .Union(Directory.EnumerateFiles(binFolder, "*.exe"));

                    assemblies.AddRange(binAssemblies);
                }

                var aggregateCatalog = new AggregateCatalog();

                var assemblyCatalogs = assemblies.Select(x => new AssemblyCatalog(x));
                foreach (var assemblyCatalog in assemblyCatalogs)
                {
                    aggregateCatalog.Catalogs.Add(assemblyCatalog);
                }

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

        private static bool ShouldInitDrirectoryCatalog(ScriptCsArgs args)
        {
            return args.Repl || !string.IsNullOrWhiteSpace(args.ScriptName);
        }
    }
}