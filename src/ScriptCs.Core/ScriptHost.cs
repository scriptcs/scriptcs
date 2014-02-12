using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost : IScriptHost, IRequirer
    {
        private readonly IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment)
        {
            _scriptPackManager = scriptPackManager;
            Env = environment;
        }

        public ScriptEnvironment Env { get; private set; }

        public T Require<T>() where T : IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

        public IScriptPackContext Require(Type context)
        {
            return _scriptPackManager.Get(context);
        }
    }
}