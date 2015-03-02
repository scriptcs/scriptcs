namespace ScriptCs.Argument
{
    public interface IArgumentHandler
    {
        ScriptCsArgs Parse(ScriptCsArgs scriptCsArgs, string[] args);
    }
}