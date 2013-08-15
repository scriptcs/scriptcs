namespace ScriptCs.Argument
{
    public class ArgumentParseResult
    {
        public ArgumentParseResult(string[] arguments, ScriptCsArgs commandArguments, string[] scriptArguments)
        {
            Arguments = arguments;
            CommandArguments = commandArguments;
            ScriptArguments = scriptArguments;
        }

        public string[] Arguments { get; private set; }
        public ScriptCsArgs CommandArguments { get; private set; }
        public string[] ScriptArguments { get; private set; }
    }
}