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

            var engineType = config.Cache ? typeof(CSharpPersistentEngine) : typeof(CSharpScriptEngine);
            engineType = config.Debug ? typeof(CSharpScriptInMemoryEngine) : engineType;
            engineType = config.IsRepl ? typeof(CSharpReplEngine) : engineType;
            config.Overrides[typeof(IScriptEngine)] = engineType;
        }
    }
}
