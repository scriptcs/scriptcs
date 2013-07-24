using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptExecutor
    {
        void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks);
        ScriptResult Execute(string script, string[] scriptArgs);
        ScriptResult Execute(string script);
        void Terminate();
    }
}