using PowerArgs;
using ScriptCs.Command;
using System;

namespace ScriptCs
{
    internal class Program
    {
        private static int Main(string[] args) 
        {
            ScriptCsArgs commandArgs;

            try
            {
                commandArgs = Args.Parse<ScriptCsArgs>(args);
            }
            catch (ArgException ex) 
            {
                commandArgs = new ScriptCsArgs();

                Console.WriteLine(ex.Message);
            }

            var debug = commandArgs.DebugFlag;
            var compositionRoot = new CompositionRoot(debug);
            compositionRoot.Initialize();
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