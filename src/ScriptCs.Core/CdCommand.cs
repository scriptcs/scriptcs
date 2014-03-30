using System.IO;
using System.Text.RegularExpressions;

namespace ScriptCs.Contracts
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

            var m = Regex.Match(args[0].ToString(), @":cd\s+(.*)");

            var relativePath = m.Groups[1].Value;

            repl.FileSystem.CurrentDirectory = Path.Combine(repl.FileSystem.CurrentDirectory, relativePath);

            return null;
        }
    }
}