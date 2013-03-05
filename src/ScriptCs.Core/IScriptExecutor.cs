using Scriptcs.Contracts;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Scriptcs
{
    [InheritedExport]
    public interface IScriptExecutor
    {
        void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptcsRecipe> recipes);
    }
}