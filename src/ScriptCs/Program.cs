using System;
using System.Linq;
using PowerArgs;

namespace ScriptCs
{
    using log4net;

    internal class Program
    {
        private static int Main(string[] args)
        {
            ILog logger = null;
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
                var logLevel = commandArgs.LogLevel;
                var compositionRoot = new CompositionRoot(debug, logLevel);
                compositionRoot.Initialize();
                logger = compositionRoot.GetLogger();
            
                logger.Debug("Creating ScriptServiceRoot");
                var scriptServiceRoot = compositionRoot.GetServiceRoot(); 
 
                var workingDirectory = scriptServiceRoot.FileSystem.GetWorkingDirectory(script);
                var paths = scriptServiceRoot.PackageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    logger.InfoFormat("Found assembly reference: {0}", path);
                }

                scriptServiceRoot.Executor.Execute(script, paths, scriptServiceRoot.ScriptPackResolver.GetPacks());
                return 0;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Fatal(ex.Message);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }

                return -1;
            }
        }
    }
}