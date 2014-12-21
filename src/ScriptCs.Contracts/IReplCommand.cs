namespace ScriptCs.Contracts
{
    public interface IReplCommand
    {
        string Description { get; }

        string CommandName { get; }

        object Execute(IRepl repl, object[] args);
    }
}