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

            const string unexpectedArgumentMessage = "Unexpected Argument: ";
            try 
            {
                commandArgs = Args.Parse<ScriptCsArgs>(args);
            }
            catch (ArgException ex) 
            {
                commandArgs = new ScriptCsArgs();

                if (ex.Message.StartsWith(unexpectedArgumentMessage)) 
                {
                    var token = ex.Message.Substring(unexpectedArgumentMessage.Length);
                    Console.WriteLine("Parameter \"{0}\" is not supported!", token);
                }
                else 
                {
                    Console.WriteLine(ex.Message);
                }
            }

            var debug = commandArgs.DebugFlag;
            var scriptProvided = !string.IsNullOrWhiteSpace(commandArgs.ScriptName);
            var compositionRoot = new CompositionRoot(debug, scriptProvided);
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