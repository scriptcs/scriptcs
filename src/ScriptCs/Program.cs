using System;
using System.IO;
using System.Runtime;
using ScriptCs.Argument;
using ScriptCs.Command;

namespace ScriptCs
{
    internal static class Program
    {
        [LoaderOptimizationAttribute(LoaderOptimization.MultiDomain)]
        private static int Main(string[] args)
        {
            ProfileOptimization.SetProfileRoot(typeof(Program).Assembly.Location);
            ProfileOptimization.StartProfile(typeof(Program).Assembly.GetName().Name + ".profile");

            var console = new ScriptConsole();

            var parser = new ArgumentHandler(new ArgumentParser(console), new ConfigFileParser(console), new FileSystem());
            var arguments = parser.Parse(args);
            var commandArgs = arguments.CommandArguments;
            var scriptArgs = arguments.ScriptArguments;

            var scriptServiceRoot = commandArgs.CreateServices(console);
            if (scriptServiceRoot == null)
            {
                return 1;
            }

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