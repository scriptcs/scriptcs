using System;
using System.Linq;
using Autofac.Builder;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var commandArgs = Args.Parse<ScriptCsArgs>(args);

                if (!commandArgs.IsValid())
                {
                    Console.WriteLine(ArgUsage.GetUsage<ScriptCsArgs>());
                    return -1;
                }

                var script = commandArgs.ScriptName;
                var debug = commandArgs.DebugFlag;
                var compositionRoot = new CompositionRoot(debug);
                compositionRoot.Initialize();
                var scriptServiceRoot = compositionRoot.GetServiceRoot(); 

                var workingDirectory = scriptServiceRoot.FileSystem.GetWorkingDirectory(script);
                var paths = scriptServiceRoot.PackageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    Console.WriteLine("Found assembly reference: " + path);
                }

                var result = scriptServiceRoot.Executor.Execute(script, paths, scriptServiceRoot.ScriptPackResolver.GetPacks());
                return result is int ? (int)result : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}