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

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand();

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }
    }
}