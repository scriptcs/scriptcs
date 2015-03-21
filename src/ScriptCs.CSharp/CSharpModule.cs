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

            var engineType = config.Debug ? typeof(CSharpScriptInMemoryEngine) : typeof(CSharpScriptEngine);
            engineType = config.IsRepl ? typeof (CSharpReplEngine) : engineType;
            config.Overrides[typeof(IScriptEngine)] = engineType;
        }
    }
}
