using PowerArgs;
using ScriptCs.Command;
using System;

namespace ScriptCs
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine(string.Format("ScriptCs version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            var commandArgs = Args.Parse<ScriptCsArgs>(args);

            var debug = commandArgs.DebugFlag;
            var compositionRoot = new CompositionRoot(debug);
            compositionRoot.Initialize();
            var scriptServiceRoot = compositionRoot.GetServiceRoot();

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(commandArgs);

            return command.Execute();
        }
    }
}