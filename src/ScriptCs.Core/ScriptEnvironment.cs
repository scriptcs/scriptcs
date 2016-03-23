using System;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptEnvironment : IScriptEnvironment
    {
        private readonly IConsole _console;
        private readonly Printers _printers;

        public ScriptEnvironment(string[] scriptArgs, IConsole console, Printers printers)
        {
            _console = console;
            _printers = printers;
            ScriptArgs = scriptArgs;
        }

        public IReadOnlyList<string> ScriptArgs { get; private set; }

        public void AddCustomPrinter<T>(Func<T, string> printer)
        {
            _console.WriteLine("Adding custom printer");
            _printers[typeof(T)] = x => printer((T) x);
        }
    }
}