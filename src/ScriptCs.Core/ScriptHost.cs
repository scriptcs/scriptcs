using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost
    {
        private IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager)
        {
            _scriptPackManager = scriptPackManager;
        }

        public T Require<T>() where T:IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

    }
}
