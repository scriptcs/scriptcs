using System;
using System.ComponentModel.Composition.Hosting;
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
        private IContainer _container;

        public CompositionRoot(bool debug)
        {
            _debug = debug;
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

            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.pack.dll");
            builder.RegisterComposablePartCatalog(catalog);
            _container = builder.Build();
        }

        public ScriptServiceRoot GetServiceRoot()
        {
            return _container.Resolve<ScriptServiceRoot>();
        }
    }
}