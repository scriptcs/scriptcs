using PowerArgs;
using ScriptCs.Command;

namespace ScriptCs
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var commandArgs = Args.Parse<ScriptCsArgs>(args);

            var debug = commandArgs.DebugFlag;
            var compositionRoot = new CompositionRoot(debug);
            compositionRoot.Initialize();
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