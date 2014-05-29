namespace ScriptCs.Contracts
{
    public interface IReplCommand
    {
        string CommandName { get; }

        object Execute(IScriptExecutor repl, object[] args);
    }
}