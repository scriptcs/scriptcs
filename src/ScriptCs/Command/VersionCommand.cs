using System;

namespace ScriptCs.Command
{
    internal class VersionCommand : IVersionCommand
    {
        public int Execute()
        {
            Console.WriteLine(string.Format("ScriptCs version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            return 0;
        }
    }
}
