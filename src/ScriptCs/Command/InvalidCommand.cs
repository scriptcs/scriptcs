using System;
using PowerArgs;

namespace ScriptCs.Command
{
    internal class InvalidCommand : IInvalidCommand
    {
        public CommandResult Execute()
        {
            ArgUsage.GetStyledUsage<ScriptCsArgs>().Write();
            return CommandResult.Error;
        }
    }
}