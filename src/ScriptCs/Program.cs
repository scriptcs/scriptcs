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
           
            var scriptServiceRoot = compositionRoot.GetServiceRoot();
            scriptServiceRoot.Logger.Debug("ScriptServiceRoot created");

            // todo: second parse?!
            var parserResult = scriptServiceRoot.ArgumentHandler.Parse(args);

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(parserResult.CommandArguments, parserResult.ScriptArguments);

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }
    }
}