using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class CdCommand : IReplCommand
    {
        public string Description => "Changes the working directory to the path provided.";

        public string CommandName => "cd";

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            if (args == null || args.Length == 0)
            {
                return null;
            }

            var path = args[0].ToString();

            repl.FileSystem.CurrentDirectory = Path.GetFullPath(Path.Combine(repl.FileSystem.CurrentDirectory, path));

            return null;
        }
    }
}