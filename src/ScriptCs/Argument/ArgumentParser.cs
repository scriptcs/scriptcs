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
            const string unexpectedArgumentMessage = "Unexpected Argument: ";

            try
            {
                commandArgs = Args.Parse<ScriptCsArgs>(args);

                //If there are any args that are -log or -writecex, they are also REPL
                if (args.Length == 1 && args.Any(x => x.ToLowerInvariant() == "-writecex")
                    || (args.Length == 2 && args.Any(x => x.ToLowerInvariant() == "-log"))
                    || args.Length == 3 && args.Any(x => x.ToLowerInvariant() == "-log") && args.Any(x => x.ToLowerInvariant() == "-writecex"))
                {
                    commandArgs.Repl = true;
                }
            }
            catch (ArgException ex)
            {
                if (ex.Message.StartsWith(unexpectedArgumentMessage))
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