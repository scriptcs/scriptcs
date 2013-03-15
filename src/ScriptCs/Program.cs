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
            var compositionRoot = new CompositionRoot(debug);
            compositionRoot.Initialize();
            var scriptServiceRoot = compositionRoot.GetServiceRoot(); 
 
            try
            {
                var workingDirectory = scriptServiceRoot.FileSystem.GetWorkingDirectory(script);
                var paths = scriptServiceRoot.PackageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    Console.WriteLine("Found assembly reference: " + path);
                }

                scriptServiceRoot.Executor.Execute(script, paths, scriptServiceRoot.ScriptPackResolver.GetPacks());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}