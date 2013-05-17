using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
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
                    typeof (FileSystem),
                    typeof (PackageAssemblyResolver),
                    typeof (PackageContainer),
                    typeof (FilePreProcessor),
                    typeof (ScriptPackResolver),
                    typeof (NugetInstallationProvider),
                    typeof (PackageInstaller),
                    typeof (ReplConsole),
                    typeof (AssemblyName)
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

            if (_shouldInitDrirectoryCatalog) 
            {
                var scriptPath = Path.Combine(Environment.CurrentDirectory, "bin");
                if (Directory.Exists(scriptPath)) 
                {
                    var catalog = new DirectoryCatalog(scriptPath);
                    builder.RegisterComposablePartCatalog(catalog);
                }
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