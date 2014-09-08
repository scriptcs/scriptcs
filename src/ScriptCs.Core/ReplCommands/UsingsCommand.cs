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

        public object Execute(IScriptExecutor repl, object[] args)
        {
            return repl.Namespaces;
        }
    }
}