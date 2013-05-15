using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        ScriptExecutionResult Execute(string code, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession);
    }
}
