using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ReferencesCommand : IReplCommand
    {
        public string CommandName
        {
            get { return "references"; }
        }

        public string Description
        {
            get { return "Displays a list of assemblies referenced from the REPL context."; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            return repl.References != null
                ? repl.References.Assemblies.Select(x => x.FullName).Union(repl.References.Paths)
                : null;
        }
    }
}