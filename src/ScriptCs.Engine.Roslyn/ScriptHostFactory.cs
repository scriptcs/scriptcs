using System.Collections.Generic;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;

namespace ScriptCs.Engine.Roslyn
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        public ScriptHost CreateScriptHost(IEnumerable<IScriptPackContext> contexts)
        {
            return new ScriptHost(contexts);
        }
    }
}