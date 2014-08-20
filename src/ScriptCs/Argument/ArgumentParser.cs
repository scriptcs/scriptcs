using System;
using System.Linq;
using PowerArgs;
using ScriptCs.Contracts;

namespace ScriptCs.Argument
{
    public class ArgumentParser : IArgumentParser
    {
        private readonly IConsole _console;

        public ArgumentParser(IConsole console)
        {
            _console = console;
        }

        public ScriptCsArgs Parse(string[] args)
        {
            //no args initialized REPL
            if (args == null || args.Length <= 0) 
                return new ScriptCsArgs { Repl = true };

            ScriptCsArgs commandArgs = null;
            const string unexpectedArgumentMessage = "unexpected named argument: ";

            try
            {
                var installArgPosition = Array.FindIndex(args, x => x.ToLowerInvariant() == "-install");
                string packageVersion = null;
                if (installArgPosition == 0 && args.Length > 2 && !args[installArgPosition+2].StartsWith("-"))
                {
                    packageVersion = args[installArgPosition + 2];
                    var argsList = args.ToList();
                    argsList.RemoveAt(installArgPosition + 2);
                    args = argsList.ToArray();
                }

                commandArgs = Args.Parse<ScriptCsArgs>(args);

                //if there is only 1 arg and it is a loglevel, it's also REPL
                if (args.Length == 2 && args.Any(x => x.ToLowerInvariant() == "-loglevel" || x.ToLowerInvariant() == "-log"))
                {
                    commandArgs.Repl = true;
                }
                
                if (!string.IsNullOrWhiteSpace(packageVersion))
                    commandArgs.PackageVersion = packageVersion;
            }
            catch(ArgException ex)
            {
                if(ex.Message.ToLower().StartsWith(unexpectedArgumentMessage))
                {
                    var token = ex.Message.Substring(unexpectedArgumentMessage.Length);
                    _console.WriteLine(string.Format("Parameter \"{0}\" is not supported!", token));
                }
                else
                {
                    _console.WriteLine(ex.Message);
                }
            }

            return commandArgs;
        }
    }
}