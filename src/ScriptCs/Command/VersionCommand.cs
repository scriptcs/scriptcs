using System;

namespace ScriptCs.Command
{
    internal class VersionCommand : IVersionCommand
    {
        public CommandResult Execute()
        {
            Console.WriteLine("ScriptCs version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            return CommandResult.Success;
        }
    }
}
