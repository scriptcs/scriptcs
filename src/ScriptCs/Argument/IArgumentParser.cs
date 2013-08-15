namespace ScriptCs.Argument
{
    public interface IArgumentParser
    {
        ScriptCsArgs Parse(string[] args);
    }
}