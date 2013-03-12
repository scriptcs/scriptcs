using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using ScriptCs.Exceptions;

namespace ScriptCs
{
    using System.ComponentModel.Composition.Registration;
    using System.Reflection;

    using ScriptCs.Contracts;
    using ScriptCs.Package;

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

            var container = ConfigureMef(debug);
            var fileSystem = container.GetExportedValue<IFileSystem>();
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

        private static void WriteUsageMessage()
        {
            Console.WriteLine("Usage:\r\n\r\nscriptcs csxFile [-debug]\r\n");
        }

        private static CompositionContainer ConfigureMef(bool debug)
        {
            var conventions = SetupMefConventions(debug);

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly, conventions));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ScriptExecutor).Assembly, conventions));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.pack.dll", conventions));
            
            return new CompositionContainer(catalog);
        }

        private static RegistrationBuilder SetupMefConventions(bool debug)
        {
            var conventions = new RegistrationBuilder();

            conventions.ForTypesDerivedFrom<ICompiledDllDebugger>().Export<ICompiledDllDebugger>();
            conventions.ForTypesDerivedFrom<IScriptHostFactory>().Export<IScriptHostFactory>();
            conventions.ForTypesDerivedFrom<IFileSystem>().Export<IFileSystem>();
            conventions.ForTypesDerivedFrom<IPackageAssemblyResolver>().Export<IPackageAssemblyResolver>();
            conventions.ForTypesDerivedFrom<IScriptEngine>()
                       .Export<IScriptEngine>()
                       .SelectConstructor(
                           constructors =>
                           constructors.First(
                               c => c.GetParameters().Length == constructors.Min(ctor => ctor.GetParameters().Length)));
            conventions.ForTypesDerivedFrom<IPackageContainer>().Export<IPackageContainer>();
            conventions.ForTypesDerivedFrom<IScriptPack>().Export<IScriptPack>();

            if (debug)
            {
                conventions.ForType<DebugScriptExecutor>().Export<IScriptExecutor>();
                conventions.ForType<DebugFilePreProcessor>().Export<IFilePreProcessor>();
            }
            else
            {
                conventions.ForType<ScriptExecutor>().Export<IScriptExecutor>();
                conventions.ForType<FilePreProcessor>().Export<IFilePreProcessor>();
            }

            return conventions;
        }
    }
}