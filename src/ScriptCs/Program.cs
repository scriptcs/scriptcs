using System;
using System.Linq;
using Autofac.Builder;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var commandArgs = Args.Parse<ScriptCsArgs>(args);

            if (!commandArgs.IsValid())
            {
                Console.WriteLine(ArgUsage.GetUsage<ScriptCsArgs>());
                return;
            }

            var script = commandArgs.ScriptName;
            var debug = commandArgs.DebugFlag;
            var bootstrapper = new Bootstrapper(debug);
            bootstrapper.Initialize();
            var root = bootstrapper.GetCompositionRoot(); 
 
            try
            {
                var workingDirectory = root.FileSystem.GetWorkingDirectory(script);
                var paths = root.PackageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    Console.WriteLine("Found assembly reference: " + path);
                }

                root.Executor.Execute(script, paths, root.ScriptPackResolver.GetPacks());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}