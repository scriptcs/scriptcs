using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost : IScriptHost
    {
        private readonly IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment)
        {
            Guard.AgainstNullArgument("scriptPackManager", scriptPackManager);

            _scriptPackManager = scriptPackManager;
            Env = environment;
        }

        public IScriptEnvironment Env { get; private set; }

        public T Require<T>() where T : IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }
    }
}
