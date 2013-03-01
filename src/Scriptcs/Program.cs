using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using System.IO;

namespace Scriptcs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:\r\n\r\nscriptcs [file] [recipe1] [recipe2] ...\r\n");
                return;
            }

            var script = args[0];

            var recipes = new List<string>();
            if (args.Length > 1)
            {
                for (int i = 1; i <= args.Length; i++)
                {
                    recipes.Add(args[i]);
                }
            }

            var container = ConfigureMef();
            var fileSystem = container.GetExportedValue<IFileSystem>();
            var resolver = container.GetExportedValue<IPackageAssemblyResolver>();
            var paths = resolver.GetAssemblyNames();

            var executor = container.GetExportedValue<IScriptExecutor>();
            var recipeManager = new RecipeManager(container);

            executor.Execute(script, paths, recipeManager.GetReceipes(recipes));
        }

        private static CompositionContainer ConfigureMef()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));

            var recipesFolder = AppDomain.CurrentDomain.BaseDirectory + @"\Recipes";
            if (!Directory.Exists(recipesFolder))
                Directory.CreateDirectory(recipesFolder);

            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory + @"\Recipes"));
            return new CompositionContainer(catalog);
        }
    }
}