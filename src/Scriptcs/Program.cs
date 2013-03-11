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
            var options = args.Where(x => x.StartsWith("-")).ToList();
            var script = args.Where(x => !x.StartsWith("-")).FirstOrDefault();
            var recipes = args.Where(x => !x.StartsWith("-")).Skip(1).ToList();

            if (args.Length == 0) {
                Console.WriteLine(@"usage: scriptcs [options] [file] [recipe1] [recipe2] ...

    -debug            enables debugging of the script
");
                return;
            }

            var container = ConfigureMef();
            var fileSystem = container.GetExportedValue<IFileSystem>();
            var resolver = container.GetExportedValue<IPackageAssemblyResolver>();
            var paths = resolver.GetAssemblyNames();

            var executorName = options.Contains("-debug") ? "-debug" : "-run";

            var executor = container.GetExportedValue<IScriptExecutor>(executorName);
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