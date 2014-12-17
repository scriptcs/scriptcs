namespace ScriptCs.Contracts
{
    public interface IReplCommand
    {
        string CommandName { get; }

        object Execute(IRepl repl, object[] args);
    }
}