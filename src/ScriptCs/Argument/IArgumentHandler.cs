namespace ScriptCs.Argument
{
    public interface IArgumentHandler
    {
        ArgumentParseResult Parse(string[] args);
    }
}