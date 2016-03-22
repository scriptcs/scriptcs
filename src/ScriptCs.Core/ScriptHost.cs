using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHost : IScriptHost
    {
        private readonly IScriptPackManager _scriptPackManager;

        public ScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment, IRepl repl)
        {
            Guard.AgainstNullArgument("scriptPackManager", scriptPackManager);

            _scriptPackManager = scriptPackManager;
            Env = environment;
            Repl = repl;
        }

        public IScriptEnvironment Env { get; private set; }
        public IRepl Repl { get; private set; }
        public T Require<T>() where T : IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }
    }
}
