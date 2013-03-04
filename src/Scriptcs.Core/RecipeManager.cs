using Scriptcs.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace Scriptcs
{
    public class RecipeManager : IRecipeManager
    {
        private readonly CompositionContainer _container;

        public RecipeManager(CompositionContainer container)
        {
            _container = container;
        }

        public IEnumerable<IScriptcsRecipe> GetReceipes(IEnumerable<string> recipeNames)
        {
            var recipes = new List<IScriptcsRecipe>();

            foreach (var recipeName in recipeNames)
            {
                var name = recipeName;
                var recipeExports = _container.GetExportedValues<Lazy<IScriptcsRecipe, IScriptcsRecipeMetadata>>()
                                              .Where(l => name == l.Metadata.Name);

                recipes.AddRange(recipeExports.Select(re => re.Value));
            }

            return recipes;
        }
    }
}