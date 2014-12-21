namespace ScriptCs.Contracts
{
    public interface IReplCommand
    {
        string Description { get; }

        string CommandName { get; }

        object Execute(IScriptExecutor repl, object[] args);
    }
}