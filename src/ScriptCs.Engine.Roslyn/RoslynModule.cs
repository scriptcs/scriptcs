using System;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    [Module("roslyn")]
    public class RoslynModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var engineType = config.IsRepl ? typeof (RoslynReplEngine) : typeof (RoslynScriptEngine);
            config.Overrides[typeof (IScriptEngine)] = engineType;
        }
    }
}
