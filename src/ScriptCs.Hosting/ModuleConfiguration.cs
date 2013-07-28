using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Hosting
{
    public class ModuleConfiguration : ScriptServiceConfiguration<IModuleConfiguration>, IModuleConfiguration
    {
        public ModuleConfiguration(bool debug, string scriptName, bool repl, LogLevel logLevel, IDictionary<Type,Object> overrides)
            :base(overrides)
        {
            Debug = debug;
            ScriptName = scriptName;
            Repl = repl;
            LogLevel = logLevel;
        }

        public bool Debug { get; private set; }
        public string ScriptName { get; private set; }
        public bool Repl { get; private set; }
        public LogLevel LogLevel { get; private set; }
    }
}
