using System.Collections.Generic;
using ScriptCs.Contracts;


namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        public ScriptHost CreateScriptHost(IEnumerable<IScriptPackContext> contexts)
        {
            return new ScriptHost(contexts);
        }
    }
}