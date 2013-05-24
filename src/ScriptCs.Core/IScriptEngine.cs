using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        ScriptResult Execute(string code, string[] scriptArgs, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession);
    }
}