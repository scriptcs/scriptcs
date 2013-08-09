namespace ScriptCs
{
    public interface IScriptHostFactory
    {
        IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment);
    }
}