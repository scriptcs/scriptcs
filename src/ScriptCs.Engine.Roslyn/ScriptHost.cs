using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class ScriptHost
    {
        private IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager)
        {
            _scriptPackManager = scriptPackManager;
        }

        public T Get<T>() where T:IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

    }
}
