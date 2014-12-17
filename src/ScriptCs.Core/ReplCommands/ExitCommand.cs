﻿using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ExitCommand : IReplCommand 
    {
        private readonly IConsole _console;

        public string CommandName
        {
            get { return "exit"; }
        }

        public ExitCommand(IConsole console)
        {
            _console = console;
        }

        public object Execute(IRepl repl, object[] args)
        {
            var response = string.Empty;
            var responseIsValid = false;

            while (!responseIsValid)
            {
                _console.Write("Are you sure you wish to exit? (y/n): ");
                response = _console.ReadLine() ?? string.Empty;
                responseIsValid = response == "y" || response == "n";
            }

            if (response == "y")
            {
                repl.Quit();
            }

            return null;
        }
    }
}
