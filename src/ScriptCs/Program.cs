using log4net;
using PowerArgs;
using ScriptCs.Command;

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

            return command.Execute();
        }
    }
}