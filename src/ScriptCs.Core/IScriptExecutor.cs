using ScriptCs.Contracts;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ScriptCs
{
    [InheritedExport]
    public interface IScriptExecutor
    {
        void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> recipes);
    }
}