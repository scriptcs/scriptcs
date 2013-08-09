namespace ScriptCs
{
    public class ScriptHostFactory : IScriptHostFactory
    {
        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment)
        {
            return new ScriptHost(scriptPackManager, environment);
        }
    }
}