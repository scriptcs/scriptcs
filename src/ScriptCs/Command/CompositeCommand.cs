using System.Collections.Generic;
using System.Linq;

namespace ScriptCs.Command
{
    public class CompositeCommand : ICompositeCommand
    {
        public CompositeCommand(params ICommand[] commands)
        {
            Commands = commands.ToList();
        }

        public List<ICommand> Commands { get; private set; }

        public CommandResult Execute()
        {
            var result = CommandResult.Success;
            foreach (var command in Commands)
            {
                result = command.Execute();

                if (result != CommandResult.Success)
                {
                    return result;
                }
            }

            return result;
        }
    }
}