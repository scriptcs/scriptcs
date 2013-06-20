using System;
using System.IO;
using Autofac;
using PowerArgs;
using ScriptCs.Command;
using System.Linq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;

namespace ScriptCs
{
    internal static class Program
    {
        private static int Main(string[] args) 
        {
<<<<<<< HEAD
            string[] scriptArgs;
            ScriptCsArgs.SplitScriptArgs(ref args, out scriptArgs);
 
            var commandArgs = ParseArguments(args);
            var configurator = new LoggerConfigurator(commandArgs.LogLevel);
            var console = new ScriptConsole();
            configurator.Configure(console);
            var logger = configurator.GetLogger();
 
            var scriptServicesBuilder = new ScriptServicesBuilder(console, logger)   .
                Debug(commandArgs.Debug).
                LogLevel(commandArgs.LogLevel).
                ScriptName(commandArgs.ScriptName).
                Repl(commandArgs.Repl);

            var modules = GetModuleList(commandArgs.Modules);
            var extension = Path.GetExtension(commandArgs.ScriptName);
            if (extension != null)
                extension = extension.Substring(1);
=======
            var parser = new ArgumentParser(args, new FileSystem());

            var compositionRoot = new CompositionRoot(parser.CommandArguments);
            compositionRoot.Initialize();
>>>>>>> Initial config arguments implementation

            scriptServicesBuilder.LoadModules(extension, modules);
            var scriptServiceRoot = scriptServicesBuilder.Build();

            var commandFactory = new CommandFactory(scriptServiceRoot);
            var command = commandFactory.CreateCommand(parser.CommandArguments, parser.ScriptArguments);

            var result = command.Execute();

            return result == CommandResult.Success ? 0 : -1;
        }
<<<<<<< HEAD

        private static string[] GetModuleList(string modulesArg)
        {
            var modules = new string[0];

            if (modulesArg != null)
                modules = modulesArg.Split(',');

            return modules;
        }

        private static ScriptCsArgs ParseArguments(string[] args)
        {
            const string UnexpectedArgumentMessage = "Unexpected Argument: ";

            //no args initialized REPL
            if (args.Length <= 0) return new ScriptCsArgs { Repl = true, LogLevel = LogLevel.Info};

            try
            {
                var scriptcsArgs = Args.Parse<ScriptCsArgs>(args);

                //if there is only 1 arg and it is a loglevel, it's also REPL
                if (scriptcsArgs.ScriptName == null && scriptcsArgs.Install == null && !scriptcsArgs.Clean && !scriptcsArgs.Help && !scriptcsArgs.Version)
                {
                    scriptcsArgs.Repl = true;
                }

                return scriptcsArgs;
            }
            catch (ArgException ex)
            {
                if (ex.Message.StartsWith(UnexpectedArgumentMessage))
                {
                    var token = ex.Message.Substring(UnexpectedArgumentMessage.Length);
                    Console.WriteLine("Parameter \"{0}\" is not supported!", token);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return new ScriptCsArgs();
        }
=======
>>>>>>> Initial config arguments implementation
    }
}