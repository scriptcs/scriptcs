using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class ScriptHost
    {
        public ScriptHost(IEnumerable<IScriptPackContext> contexts)
        {
            ScriptPackManager = new ScriptPackManager(contexts);
        }

        public ScriptPackManager ScriptPackManager { get; private set; }
    }
}
