using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ClearCommand : IReplCommand
    {
        private readonly IConsole _console;

        public string Description => "Clears the console window.";

        public ClearCommand(IConsole console)
        {
            Guard.AgainstNullArgument("console", console);

            _console = console;
        }

        public string CommandName => "clear";

        public object Execute(IRepl repl, object[] args)
        {
            _console.Clear();
            return null;
        }
    }
}