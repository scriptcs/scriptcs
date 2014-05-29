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

        public string CommandName
        {
            get { return "cwd"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            var dir = repl.FileSystem.CurrentDirectory;

            // TODO (adamralph): revert console color after writing
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.WriteLine(dir);

            return null;
        }
    }
}
