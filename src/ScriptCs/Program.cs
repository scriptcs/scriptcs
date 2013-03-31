using log4net;
using PowerArgs;
using ScriptCs.Command;
using System;

namespace ScriptCs
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            ILog logger = null;
            var commandArgs = Args.Parse<ScriptCsArgs>(args);

            var debug = commandArgs.DebugFlag;
            var logLevel = commandArgs.LogLevel;
            var compositionRoot = new CompositionRoot(debug, logLevel);
            compositionRoot.Initialize();
            logger = compositionRoot.GetLogger();
            logger.Debug("Creating ScriptServiceRoot");
            var scriptServiceRoot = compositionRoot.GetServiceRoot();

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(commandArgs);

            var result = command.Execute();

            switch (result)
            {
                case CommandResult.Success:
                    return 0;
                default:
                    return -1;
            }
        }
    }
}