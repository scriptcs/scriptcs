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
        private readonly bool _shouldInitDirectoryCatalog;
        private IContainer _container;
        private ScriptServiceRoot _scriptServiceRoot;

        public CompositionRoot(bool debug, bool useDirectoryCatalog)
        {
            _debug = debug;
            _shouldInitDirectoryCatalog = useDirectoryCatalog;
        }

        public void Initialize(string scriptName)
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

            var scriptAssemblyName = Path.GetFileNameWithoutExtension(scriptName) + ".dll";
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

            if (_shouldInitDirectoryCatalog) 
            {
                var scriptPath = Path.Combine(Environment.CurrentDirectory, "bin");
                if (Directory.Exists(scriptPath))
                {
                    var files = Directory.GetFiles(scriptPath);
                    foreach (var file in files)
                    {
                        var filename = Path.GetFileName(file);
                        if (String.Compare(scriptAssemblyName, filename, true) == 0)
                            continue;

                        var catalog = new DirectoryCatalog(scriptPath, filename);
                        builder.RegisterComposablePartCatalog(catalog);
                    }
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