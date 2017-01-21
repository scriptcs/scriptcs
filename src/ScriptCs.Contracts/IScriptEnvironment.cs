using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IScriptEnvironment
    {
        IReadOnlyList<string> ScriptArgs { get; }
        void AddCustomPrinter<T>(Func<T, string> printer);
        void Print<T>(T o);
        void Print(object o);
        string ScriptPath { get; }
        string[] LoadedScripts { get; }
        Assembly ScriptAssembly { get; }
        void Init();
    }
}