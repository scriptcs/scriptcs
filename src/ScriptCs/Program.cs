using System;

using PowerArgs;
using ScriptCs.Command;
using System.Linq;

namespace ScriptCs
{
    internal static class Program
    {
        private static int Main(string[] args) 
        {
            var compositionRoot = new CompositionRoot(args);
            compositionRoot.Initialize();

            var parser = compositionRoot.GetArgumentParser();
            var logger = compositionRoot.GetLogger();
            logger.Debug("Creating ScriptServiceRoot");
           
            var scriptServiceRoot = compositionRoot.GetServiceRoot();

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(parser.CommandArguments, parser.ScriptArguments);

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }
    }
}