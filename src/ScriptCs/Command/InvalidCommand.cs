using System;

namespace ScriptCs.Command
{
    internal class InvalidCommand : IInvalidCommand
    {
        public void Execute()
        {
            Console.WriteLine("Invalid use!");
        }
    }
}