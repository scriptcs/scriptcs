using System;
using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ModuleConfiguration : ServiceOverrides<IModuleConfiguration>, IModuleConfiguration
    {
        public ModuleConfiguration(bool cache, string scriptName, bool repl, LogLevel logLevel, bool debug, IDictionary<Type, Object> overrides)
            : base(overrides)
        {
            Cache = cache;
            ScriptName = scriptName;
            Repl = repl;
            LogLevel = logLevel;
            Debug = debug;
        }

        public bool Cache { get; private set; }

        public string ScriptName { get; private set; }

        public bool Repl { get; private set; }

        public LogLevel LogLevel { get; private set; }

        public bool Debug { get; private set; }
    }
}
