using Scriptcs.Contracts;
using System.Collections.Generic;

namespace Scriptcs.Core
{
    public interface IRecipeManager
    {
        IEnumerable<IScriptcsRecipe> GetReceipes(IEnumerable<string> recipeNames);
    }
}