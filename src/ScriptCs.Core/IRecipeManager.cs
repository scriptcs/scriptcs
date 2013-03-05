using Scriptcs.Contracts;
using System.Collections.Generic;

namespace Scriptcs
{
    public interface IRecipeManager
    {
        IEnumerable<IScriptcsRecipe> GetReceipes(IEnumerable<string> recipeNames);
    }
}