using System;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class CwdCommand : IReplCommand
    {
        private readonly IConsole _console;

        public CwdCommand(IConsole console)
        {
            Guard.AgainstNullArgument("console", console);

            _console = console;
        }

        public string Description
        {
            get { return "Displays the current working directory."; }
        }

        public string CommandName
        {
            get { return "cwd"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            var dir = repl.FileSystem.CurrentDirectory;

            var originalColor = _console.ForegroundColor;
            _console.ForegroundColor = ConsoleColor.Yellow;
            try
            {
                _console.WriteLine(dir);
            }
            finally
            {
                _console.ForegroundColor = originalColor;
            }

            return null;
        }
    }
}
