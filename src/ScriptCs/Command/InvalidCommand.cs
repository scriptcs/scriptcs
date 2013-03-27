using System;
using PowerArgs;

namespace ScriptCs.Command
{
    internal class InvalidCommand : IInvalidCommand
    {
        public CommandResult Execute()
        {
            Console.WriteLine(ArgUsage.GetUsage<ScriptCsArgs>());
            return CommandResult.Error;
        }
    }
}