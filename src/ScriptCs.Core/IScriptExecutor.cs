using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptExecutor
    {
        void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks);
        void Execute(string script, string[] scriptArgs);
        void Execute(string script);
        void Terminate();
    }
}