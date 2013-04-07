using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        public ScriptHost CreateScriptHost(IScriptPackManager scriptPackManager)
        {
            return new ScriptHost(scriptPackManager);
        }
    }
}