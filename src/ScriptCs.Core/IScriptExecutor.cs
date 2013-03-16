using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptExecutor
    {
        object Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> recipes);
    }
}