using System.Collections.Generic;
using System.ComponentModel.Composition;
using Scriptcs.Contracts;

namespace Scriptcs
{
    [InheritedExport]
    public interface IScriptExecutor
    {
        void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptcsRecipe> recipes);
    }
}