using System.IO;
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

            var configurator = new LoggerConfigurator(commandArgs.LogLevel);
            configurator.Configure(console);
            var logger = configurator.GetLogger();
 
            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger)
                .Cache(commandArgs.Cache)
                .LogLevel(commandArgs.LogLevel)
                .ScriptName(commandArgs.ScriptName)
                .Repl(commandArgs.Repl);

            var modules = GetModuleList(commandArgs.Modules);
            var extension = Path.GetExtension(commandArgs.ScriptName);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                extension = extension.Substring(1);
            }
            else if (extension == string.Empty)
            {
                console.WriteLine(string.Format("{0} is not a valid script name.", commandArgs.ScriptName));
                return 1;
            }

            scriptServicesBuilder.LoadModules(extension, modules);
            var scriptServiceRoot = scriptServicesBuilder.Build();

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(commandArgs, scriptArgs);

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }

        private static string[] GetModuleList(string modulesArg)
        {
            var modules = new string[0];

            if (modulesArg != null)
            {
                modules = modulesArg.Split(',');
            }

            return modules;
        }
    }
}