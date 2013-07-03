using ScriptCs.Argument;
using ScriptCs.Command;

namespace ScriptCs
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var console = new ScriptConsole();
            var parser = new ArgumentHandler(new ArgumentParser(console), new ConfigFileParser(console), new FileSystem());
            var arguments = parser.Parse(args);
            var commandArgs = arguments.CommandArguments;
            var scriptArgs = arguments.ScriptArguments;

            var runtime = new ScriptRuntimeBuilder().
                Debug(commandArgs.Debug).
                LogLevel(commandArgs.LogLevel).
                ScriptName(commandArgs.ScriptName).
                Repl(commandArgs.Repl).
                Build();

            runtime.Initialize();

            var logger = runtime.GetLogger();
            logger.Debug("Creating ScriptServices");
           
            var scriptServiceRoot = runtime.GetScriptServices();

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(commandArgs, scriptArgs);

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }
    }
}