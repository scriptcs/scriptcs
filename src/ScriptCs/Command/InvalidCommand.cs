using System;
using PowerArgs;

namespace ScriptCs.Command
{
    internal class InvalidCommand : IInvalidCommand
    {
        public void Execute()
        {
            Console.WriteLine(ArgUsage.GetUsage<ScriptCsArgs>());
        }
    }
}