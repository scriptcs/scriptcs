using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        public ScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new ScriptHost(scriptPackManager, scriptArgs);
        }
    }
}