using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptEnvironment : IScriptEnvironment
    {
        private readonly IConsole _console;
        private readonly Printers _printers;
        private readonly IScriptInfo _scriptInfo;

        public ScriptEnvironment(string[] scriptArgs, IConsole console, Printers printers, IScriptInfo scriptInfo = null )
        {
            _console = console;
            _printers = printers;
            _scriptInfo = scriptInfo;            
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

        public string ScriptPath => _scriptInfo.ScriptPath;

        public string[] LoadedScripts => _scriptInfo.LoadedScripts.ToArray();

        public Assembly ScriptAssembly { get; private set; }

        private bool _initialized;
        public void Initialize()
        {
            if (!_initialized)
            {
                ScriptAssembly = Assembly.GetCallingAssembly();
                _initialized = true;
            }
        }
    }
}
