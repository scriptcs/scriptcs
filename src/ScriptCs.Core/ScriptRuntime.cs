using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

using Autofac;
using Autofac.Integration.Mef;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class ScriptRuntime
    {
        /*
        public ScriptRuntime(ILog logger, IConsole console, Type scriptEngineType, Type scriptExecutorType, bool initDirectoryCatalog, IDictionary<Type, object> overrides)
            : this(new RuntimeContainerFactory(logger, console, scriptEngineType, scriptExecutorType, initDirectoryCatalog))
        {
            
        }
        */
         
        public ScriptRuntime(IRuntimeContainerFactory containerFactory)
        {
            var container = containerFactory.Container;
            ScriptServices = container.Resolve<ScriptServices>();
            Logger = container.Resolve<ILog>();
        }

        public ScriptServices ScriptServices { get; private set; }

        public ILog Logger { get; private set; }
    }
}

