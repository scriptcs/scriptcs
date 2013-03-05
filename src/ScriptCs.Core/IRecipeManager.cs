using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IRecipeManager
    {
        IEnumerable<IScriptCsRecipe> GetReceipes(IEnumerable<string> recipeNames);
    }
}