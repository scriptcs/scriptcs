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
            _console.WriteLine("Adding custom printer for " + typeof(T).Name);
            _printers.AddCustomPrinter<T>(printer);
        }

        public void Print(object o)
        {
            _console.WriteLine(_printers.GetStringFor(o));
        }

        public void Print<T>(T o)
        {
            _console.WriteLine(_printers.GetStringFor<T>(o));
        }

    }
}