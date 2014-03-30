using System;

namespace ScriptCs.Contracts
{
    public class CwdCommand : IReplCommand
    {
        private readonly IConsole _console;

        public CwdCommand(IConsole console)
        {
            _console = console;
        }

        public string CommandName
        {
            get { return "cwd"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            var dir = repl.FileSystem.CurrentDirectory;
            _console.ForegroundColor = ConsoleColor.Yellow;
            _console.WriteLine(dir);

            return null;
        }
    }
}