using System;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        bool CacheAssembly { get; set; }
        DateTime? AssemblyCacheDate { get; set; }
        string AssemblyName { get; set; }
        bool CanExecuteCached();
        void CleanUpCachedAssembly();
        void ExecuteCached(IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession);
        void Execute(string code, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession);
    }
}