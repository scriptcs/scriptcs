using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using Autofac;
using Autofac.Integration.Mef;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class CompositionRoot
    {
        private readonly bool _debug;
        private readonly bool _shouldInitDrirectoryCatalog;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;

        public CompositionRoot(bool debug, bool useDirectoryCatalog)
        {
            _debug = debug;
            _shouldInitDrirectoryCatalog = useDirectoryCatalog;
        }

        public void Initialize()
        {
            var builder = new ContainerBuilder();
            var types = new[]
                {
                    typeof (ScriptHostFactory),
                    typeof (FileSystem),
                    typeof (PackageAssemblyResolver),
                    typeof (PackageContainer),
                    typeof (FilePreProcessor),
                    typeof (ScriptPackResolver),
                    typeof (NugetInstallationProvider),
                    typeof (PackageInstaller)
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
    }
}