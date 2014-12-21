using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class AliasCommand : IReplCommand
    {
        private readonly IConsole _console;

        public AliasCommand(IConsole console)
        {
            _console = console;
        }

        public string CommandName
        {
            get { return "alias"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            if (args == null || args.Length != 2)
            {
                return null;
            }

            var replInstance = repl as Repl;

            if (replInstance == null)
            {
                return null;
            }

            var originalCommandName = args[0].ToString();
            var aliasName = args[1].ToString();

            if (replInstance.Commands.Any(x => x.Key.ToLowerInvariant() == aliasName.ToLowerInvariant()))
            {
                return null;
            }

            var oldReplCommand = replInstance.Commands[originalCommandName];
            replInstance.Commands[aliasName] = oldReplCommand;
            _console.WriteLine(string.Format("Aliased {0} as {1}", originalCommandName, aliasName));

            return null;
        }
    }
}