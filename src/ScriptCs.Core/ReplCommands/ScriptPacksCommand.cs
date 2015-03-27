using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class ScriptPacksCommand : IReplCommand
    {
        public string Description
        {
            get { return "Displays information about script packs available in the REPL session"; }
        }

        public string CommandName
        {
            get { return "scriptpacks"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}