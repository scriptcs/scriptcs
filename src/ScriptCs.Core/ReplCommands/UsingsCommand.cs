using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class UsingsCommand : IReplCommand
    {
        public string Description
        {
            get { return "Displays a list of namespaces imported into REPL context."; }
        }

        public string CommandName
        {
            get { return "usings"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            var namespaces = repl.Namespaces;

            if (repl.ScriptPackSession == null || repl.ScriptPackSession.Namespaces == null || !repl.ScriptPackSession.Namespaces.Any())
                return namespaces;

            return namespaces.Union(repl.ScriptPackSession.Namespaces).OrderBy(x => x);
        }
    }
}