namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        public ScriptHost CreateScriptHost(string scriptArgs, IScriptPackManager scriptPackManager)
        {
            return new ScriptHost(scriptArgs, scriptPackManager);
        }
    }
}