using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IReplEngine : IScriptEngine
    {
        ICollection<string> GetLocalVariables(ScriptPackSession scriptPackSession);
    }
}