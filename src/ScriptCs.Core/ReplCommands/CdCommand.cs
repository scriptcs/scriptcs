using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class CdCommand : IReplCommand
    {
        public string CommandName
        {
            get { return "cd"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            if (args == null || args.Length == 0) return null;

            var relativePath = args[0].ToString();

            if (!relativePath.EndsWith(@"\"))
            {
                relativePath += @"\";
            }

            repl.FileSystem.CurrentDirectory = Path.GetFullPath(Path.Combine(repl.FileSystem.CurrentDirectory, relativePath));

            return null;
        }
    }
}