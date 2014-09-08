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

        public object Execute(IScriptExecutor repl, object[] args)
        {
            if (repl.References != null)
            {
                return repl.References.Assemblies.Select(x => x.FullName).Union(repl.References.PathReferences);
            }

            return null;
        }
    }
}