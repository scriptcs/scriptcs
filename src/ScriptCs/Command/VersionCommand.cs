using System.Diagnostics;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class VersionCommand : IVersionCommand
    {
        private readonly IConsole _console;

        public VersionCommand(IConsole console)
        {
            _console = console;
        }

        public CommandResult Execute()
        {
            var message = string.Format("scriptcs version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            _console.WriteLine(message);

            return CommandResult.Success;
        }
    }
}
