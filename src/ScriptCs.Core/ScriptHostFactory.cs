using ScriptCs.Contracts;
using System;
namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        private IRepl _repl;

        public void SetRepl(IRepl repl)
        {
            _repl = repl;
        }

        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new ScriptHost(scriptPackManager, new ScriptEnvironment(scriptArgs), _repl);
        }
    }
}
