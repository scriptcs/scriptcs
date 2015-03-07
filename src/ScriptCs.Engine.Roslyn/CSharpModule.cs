using System;
using ScriptCs.Contracts;

namespace ScriptCs.CSharp
{
    [Module("csharp")]
    public class CSharpModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var engineType = config.IsRepl ? typeof (CSharpReplEngine) : typeof (CSharpScriptEngine);
            config.Overrides[typeof (IScriptEngine)] = engineType;
        }
    }
}
