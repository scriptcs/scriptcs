using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ResetCommand : IReplCommand
    {
        public string Description
        {
            get { return "Resets the REPL state. All local variables and member definitions are cleared."; }
        }

        public string CommandName
        {
            get { return "reset"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            repl.Reset();
            return null;
        }
    }
}