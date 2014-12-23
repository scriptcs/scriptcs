using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ClearCommand : IReplCommand
    {
        private readonly IConsole _console;

        public string Description
        {
            get { return "Clears the console window."; }
        }

        public ClearCommand(IConsole console)
        {
            Guard.AgainstNullArgument("console", console);

            _console = console;
        }

        public string CommandName
        {
            get { return "clear"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            _console.Clear();
            return null;
        }
    }
}