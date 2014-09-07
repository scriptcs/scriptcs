using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ShowCommand : IReplCommand
    {
        public string CommandName
        {
            get { return "show"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            if (args == null || args.Length == 0)
            {
                return null;
            }

            var operation = args[0] as string;

            if (operation == "refs")
            {
                return repl.References.Assemblies.Select(x => x.FullName).Union(repl.References.PathReferences);
            }

            if (operation == "usings")
            {
                return repl.Namespaces;
            }

            if (operation == "vars")
            {
                var replEngine = repl.ScriptEngine as IReplEngine;
                if (replEngine != null)
                {
                    return replEngine.LocalVariables;
                }
            }

            return "Invalid switch";
        }
    }
}