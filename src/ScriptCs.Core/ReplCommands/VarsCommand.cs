using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class VarsCommand : IReplCommand
    {
        public string CommandName
        {
            get { return "vars"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            var replEngine = repl.ScriptEngine as IReplEngine;
            if (replEngine != null)
            {
                return replEngine.LocalVariables;
            }

            return null;
        }
    }
}