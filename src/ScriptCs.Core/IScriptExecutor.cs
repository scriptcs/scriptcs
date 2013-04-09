using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptExecutor
    {
        void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> recipes);
    }
}