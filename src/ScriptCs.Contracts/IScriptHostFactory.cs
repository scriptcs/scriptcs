namespace ScriptCs.Contracts
{
    public interface IScriptHostFactory
    {
        void SetRepl(IRepl repl);
        IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs);
    }
}
