using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using ScriptCs.Exceptions;
using ScriptCs.Package;

namespace ScriptCs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:\r\n\r\nscriptcs [file]\r\n");
                return;
            }

            var script = args[0];
            
            var container = ConfigureMef();
            var fileSystem = container.GetExportedValue<IFileSystem>();
            var packageContainer = container.GetExportedValue<IPackageContainer>();
            var resolver = container.GetExportedValue<IPackageAssemblyResolver>();

            var executor = container.GetExportedValue<IScriptExecutor>();
            var scriptPackManager = new ScriptPackResolver(container);

            try
            {
                var workingDirectory = fileSystem.GetWorkingDirectory(script);
                var paths = resolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    Console.WriteLine("Found assembly reference: " + path);
                }
                executor.Execute(script, paths, scriptPackManager.GetPacks());
            }
            catch (MissingAssemblyException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static CompositionContainer ConfigureMef()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ScriptExecutor).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory,"*.pack.dll"));
            return new CompositionContainer(catalog);
        }
    }
}