using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using ScriptCs.Exceptions;

namespace ScriptCs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                WriteUsageMessage();
                return;
            }

            var script = args[0];
            bool debug = false;

            if (args.Length == 2)
            {
                var secondParam = args[1];
                if (secondParam.Equals("-debug"))
                {
                    debug = true;
                }
                else
                {
                    Console.WriteLine("Unrecognized parameter {0}.", secondParam);
                    WriteUsageMessage();
                    return;
                }
            }

            var contractsMode = debug ? Constants.DebugContractName : Constants.RunContractName;

            var container = ConfigureMef();
            var fileSystem = container.GetExportedValue<IFileSystem>();
            var resolver = container.GetExportedValue<IPackageAssemblyResolver>();
            var executor = container.GetExportedValue<IScriptExecutor>(contractsMode);
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

        private static void WriteUsageMessage()
        {
            Console.WriteLine("Usage:\r\n\r\nscriptcs csxFile [-debug]\r\n");
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