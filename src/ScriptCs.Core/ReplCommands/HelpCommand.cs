using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class HelpCommand : IReplCommand
    {
        private readonly IConsole _console;

        public HelpCommand(IConsole console)
        {
            _console = console;
        }

        public string Description
        {
            get { return "Shows this help."; }
        }

        public string CommandName
        {
            get { return "help"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            var typedRepl = repl as Repl;
            if (typedRepl != null)
            {
                _console.WriteLine("The following commands are available in the REPL:");
                foreach (var command in typedRepl.Commands.Values.OrderBy(x => x.CommandName))
                {
                    _console.WriteLine(string.Format(":{0,-15}{1,10}", command.CommandName, command.Description));
                }
            }

            return null;
        }
    }
}