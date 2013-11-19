using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }

        string CacheDirectory { get; set; }
        
        string FileName { get; set; }
        
        ScriptResult Execute(string code, string[] scriptArgs, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession);
    }
}