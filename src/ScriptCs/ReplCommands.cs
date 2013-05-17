using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptCs
{
    public class ReplCommands
    {
        private readonly List<IReplCommand> _commands = new List<IReplCommand>();

        public ReplCommands(Repl repl, string newLine)
        {
            Register(new ExitCommand(repl));
            Register(new HelpCommand(this, newLine));
        }

        public void Register(IReplCommand command)
        {
            _commands.Add(command);
        }

        public IReplCommand Get(string line)
        {
            if (line == null)
                return null;

            if (!line.StartsWith("#"))
                return null;

            var name = line.Substring(1).ToLowerInvariant();

            return _commands.FirstOrDefault(x => x.Name == name);
        }

        public class ExitCommand : IReplCommand
        {
            private readonly Repl _repl;

            public ExitCommand(Repl repl)
            {
                _repl = repl;
            }

            public string Name { get { return "exit"; } }
            public string HelpName { get { return "Exit"; } }
            public string HelpDescription { get { return "(also ctrl-c or blank line)"; } }

            public object Execute()
            {
                _repl.Terminate();
                return null;
            }
        }

        public class HelpCommand : IReplCommand
        {
            private readonly ReplCommands _replCommands;
            private readonly string _newLine;

            public HelpCommand(ReplCommands replCommands, string newLine)
            {
                _replCommands = replCommands;
                _newLine = newLine;
            }

            public string Name { get { return "help"; } }
            public string HelpName { get { return "Help"; } }
            public string HelpDescription { get { return "Display all available REPL commands"; } }

            public object Execute()
            {
                var help = new StringBuilder();
                help.Append("REPL commands:" + _newLine);
                var commands = _replCommands._commands;
                for (int i = 0; i < commands.Count; i++)
                {
                    var cmd = commands[i];
                    help.Append(string.Format("  #{0}    {1} - {2}", cmd.Name, cmd.HelpName, cmd.HelpDescription));
                    if (i < commands.Count - 1)
                        help.Append(_newLine);
                }
                return help.ToString();
            }
        }
    }
}