using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ClearCommand : IReplCommand
    {
        private readonly IConsole _console;

        public ClearCommand(IConsole console)
        {
            _console = console;
        }

        public string CommandName
        {
            get
            {
                return "clear";
            }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            _console.Clear();
            return null;
        }
    }
}