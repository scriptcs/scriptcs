using System;
using System.ComponentModel.Composition.Hosting;
using Autofac;
using Autofac.Integration.Mef;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;

namespace ScriptCs
{
    public class Bootstrapper
    {
        private readonly bool _debug;
        private IContainer _container;

        public Bootstrapper(bool debug)
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
                    typeof (ScriptPackResolver)
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

            builder.RegisterType<CompositionRoot>().As<CompositionRoot>();

            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.pack.dll");
            builder.RegisterComposablePartCatalog(catalog);
            _container = builder.Build();
        }

        public CompositionRoot GetCompositionRoot()
        {
            return _container.Resolve<CompositionRoot>();
        }
    }
}