using Scriptcs.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scriptcs
{
    public class RecipeManager : IRecipeManager
    {
        private readonly CompositionContainer _container;

        public RecipeManager(CompositionContainer container)
        {
            _container = container;
        }

        public IEnumerable<Contracts.IScriptcsRecipe> GetReceipes(IEnumerable<string> recipeNames)
        {
            List<IScriptcsRecipe> recipes = new List<IScriptcsRecipe>();
            foreach (var recipeName in recipeNames)
            {
                var recipeExports =
                    _container.GetExportedValues<Lazy<IScriptcsRecipe, IScriptcsRecipeMetadata>>()
                        .Where(l => recipeName == l.Metadata.Name);

                recipes.AddRange(recipeExports.Select(re=>re.Value));
            }
            return recipes;
        }
    }
}
