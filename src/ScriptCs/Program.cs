using Common.Logging;
using PowerArgs;
using ScriptCs.Command;

namespace ScriptCs
{
    using System;

    internal class Program
    {
        private static int Main(string[] args) 
        {
            ILog logger = null;
            ScriptCsArgs commandArgs = null;
  
            const string unexpectedArgumentMessage = "Unexpected Argument: ";
            
            if (args.Length > 0)
            {
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
            }

            if (commandArgs == null)
            {
                commandArgs = new ScriptCsArgs() {Repl = true};
            }

            var debug = commandArgs.DebugFlag;
            var logLevel = commandArgs.LogLevel;
            var scriptProvided = !string.IsNullOrWhiteSpace(commandArgs.ScriptName) || commandArgs.Repl;
            
            var compositionRoot = new CompositionRoot(debug, scriptProvided, logLevel);
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