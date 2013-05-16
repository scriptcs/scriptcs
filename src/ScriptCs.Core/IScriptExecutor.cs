using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptExecutor
    {
        void Execute(string script, string args, IEnumerable<string> paths, IEnumerable<IScriptPack> recipes);
    }
}