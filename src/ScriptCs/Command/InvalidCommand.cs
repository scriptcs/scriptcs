using System;
using PowerArgs;

namespace ScriptCs.Command
{
    internal class InvalidCommand : IInvalidCommand
    {
        public int Execute()
        {
            Console.WriteLine(ArgUsage.GetUsage<ScriptCsArgs>());
            return -1;
        }
    }
}