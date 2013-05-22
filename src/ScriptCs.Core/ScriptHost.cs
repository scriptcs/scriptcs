using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost
    {
        private IScriptPackManager _scriptPackManager;
        public string[] ScriptArgs { get; private set; }

        public ScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            _scriptPackManager = scriptPackManager;
            ScriptArgs = scriptArgs;
        }

        public T Require<T>() where T:IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

    }
}
