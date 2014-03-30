using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public class ResetCommand : IReplCommand
    {
        public string CommandName
        {
            get
            {
                return "reset";
            }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            repl.Reset();
            return null;
        }
    }
}