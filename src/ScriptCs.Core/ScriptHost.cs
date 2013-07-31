using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost : IScriptHost
    {
        private readonly IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            _scriptPackManager = scriptPackManager;
            ScriptArgs = scriptArgs;
        }

        public string[] ScriptArgs { get; private set; }

        public T Require<T>() where T : IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }
    }
}