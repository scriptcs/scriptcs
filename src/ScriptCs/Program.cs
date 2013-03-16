using System;
using System.Linq;
using PowerArgs;

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
            var logLevel = commandArgs.LogLevel;
            var compositionRoot = new CompositionRoot(debug, logLevel);
            compositionRoot.Initialize();
            var logger = compositionRoot.GetLogger();
            
            logger.Debug("Creating ScriptServiceRoot");
            var scriptServiceRoot = compositionRoot.GetServiceRoot(); 
 
            try
            {
                var workingDirectory = scriptServiceRoot.FileSystem.GetWorkingDirectory(script);
                var paths = scriptServiceRoot.PackageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    logger.InfoFormat("Found assembly reference: {0}", path);
                }

                scriptServiceRoot.Executor.Execute(script, paths, scriptServiceRoot.ScriptPackResolver.GetPacks());
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.Message);
            }
        }
    }
}