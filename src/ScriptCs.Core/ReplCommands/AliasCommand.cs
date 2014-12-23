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

        public string Description
        {
            get { return "Allows you to alias a command with a custom name"; }
        }

        public string CommandName
        {
            get { return "alias"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            if (args == null || args.Length != 2)
            {
                _console.WriteLine("You must specifiy the command name and alias, e.g. :alias \"clear\" \"cls\"");
                return null;
            }

            var originalCommandName = args[0].ToString();
            var aliasName = args[1].ToString();

            if (repl.Commands.Any(x => x.Key.ToLowerInvariant() == aliasName.ToLowerInvariant()))
            {
                return null;
            }

            var oldReplCommand = repl.Commands[originalCommandName];
            repl.Commands[aliasName] = oldReplCommand;
            _console.WriteLine(string.Format("Aliased {0} as {1}", originalCommandName, aliasName));

            return null;
        }
    }
}