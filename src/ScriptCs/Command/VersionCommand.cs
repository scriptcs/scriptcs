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
            var assembly = typeof(Program).Assembly;

            var productVersion = FileVersionInfo
                .GetVersionInfo(assembly.Location).ProductVersion;

            _console.WriteVersion(productVersion);

            return CommandResult.Success;
        }
    }
}
