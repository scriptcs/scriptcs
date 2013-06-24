using System;
using PowerArgs;

namespace ScriptCs.Argument
{
    public class ArgumentParser : IArgumentParser
    {
        public ScriptCsArgs Parse(string[] args)
        {
            var commandArgs = new ScriptCsArgs() { Repl = true };

            if (args != null && args.Length > 0)
            {
                const string unexpectedArgumentMessage = "Unexpected Argument: ";

                try
                {
                    commandArgs = Args.Parse<ScriptCsArgs>(args);
                }
                catch (ArgException ex)
                {
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

            return commandArgs;
        }
    }
}