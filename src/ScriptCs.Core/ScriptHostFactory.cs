using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory<IScriptHost>
    {
        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new ScriptHost(scriptPackManager, scriptArgs);
        }
    }
}