namespace ScriptCs.Argument
{
    public interface IConfigFileParser
    {
        ScriptCsArgs Parse(string content);
    }
}