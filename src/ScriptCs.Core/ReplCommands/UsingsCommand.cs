using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class UsingsCommand : IReplCommand
    {
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