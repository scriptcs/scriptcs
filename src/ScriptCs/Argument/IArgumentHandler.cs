namespace ScriptCs.Argument
{
    public interface IArgumentHandler
    {
        ScriptCsArgs Parse(string[] args);
    }
}