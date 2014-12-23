using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class VarsCommand : IReplCommand
    {
        public string CommandName
        {
            get { return "vars"; }
        }

        public string Description
        {
            get { return "Displays a list of variables defined within the REPL, along with their types and values."; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            var replEngine = repl.ScriptEngine as IReplEngine;
            return replEngine != null ? replEngine.GetLocalVariables(repl.ScriptPackSession) : null;
        }
    }
}