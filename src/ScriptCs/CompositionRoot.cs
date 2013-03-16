using System;
using System.ComponentModel.Composition.Hosting;
using Autofac;
using Autofac.Integration.Mef;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace ScriptCs
{
    public class CompositionRoot
    {
        private readonly bool _debug;
        private readonly string _logLevel; 
        private IContainer _container;

        public CompositionRoot(bool debug, string logLevel)
        {
            _debug = debug;
            _logLevel = logLevel;
        }

        public void Initialize()
        {
            var builder = new ContainerBuilder();

            var logger = CreateLogger();
            builder.RegisterInstance<ILog>(logger);

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

            builder.RegisterType<ScriptServiceRoot>().As<ScriptServiceRoot>();

            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.pack.dll");
            builder.RegisterComposablePartCatalog(catalog);
            _container = builder.Build();
        }

        public ScriptServiceRoot GetServiceRoot()
        {
            return _container.Resolve<ScriptServiceRoot>();
        }

        public ILog GetLogger()
        {
            return _container.Resolve<ILog>();
        }

        private ILog CreateLogger()
        {
            const string Pattern = "%-5level Thread[%thread]: %message%newline";
            const string LoggerName = "scriptcs";
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = LogManager.GetLogger(LoggerName);
            var consoleAppender = new ConsoleAppender
                                                  {
                                                      Layout = new PatternLayout(Pattern),
                                                      Threshold = hierarchy.LevelMap[_logLevel]
                                                  };

            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            return logger;
        }
    }
}