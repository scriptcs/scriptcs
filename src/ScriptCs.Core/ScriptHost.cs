using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost
    {
        private IScriptPackManager _scriptPackManager;
        public string[] Args { get; private set; }

        public ScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            _scriptPackManager = scriptPackManager;
            Args = scriptArgs;
        }

        public T Require<T>() where T:IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

    }
}
