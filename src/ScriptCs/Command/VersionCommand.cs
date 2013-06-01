using System.Reflection;

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
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            _console.WriteLine(string.Format("scriptcs version {0}", version));

            return CommandResult.Success;
        }
    }
}
