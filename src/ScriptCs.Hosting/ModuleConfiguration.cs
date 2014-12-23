using System;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ModuleConfiguration : ServiceOverrides<IModuleConfiguration>, IModuleConfiguration
    {
        public ModuleConfiguration(
            bool cache,
            string scriptName,
            bool isRepl,
            LogLevel logLevel,
            bool debug,
            IDictionary<Type, Object> overrides)
            : base(overrides)
        {
            Cache = cache;
            ScriptName = scriptName;
            IsRepl = isRepl;
            LogLevel = logLevel;
            Debug = debug;
        }

        public bool Cache { get; private set; }

        public string ScriptName { get; private set; }

        public bool IsRepl { get; private set; }

        public LogLevel LogLevel { get; private set; }

        public bool Debug { get; private set; }
    }
}
