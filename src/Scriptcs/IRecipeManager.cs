using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scriptcs.Contracts;

namespace Scriptcs
{
    public interface IRecipeManager
    {
        IEnumerable<IScriptcsRecipe> GetReceipes(IEnumerable<string> recipeNames);
    }
}
