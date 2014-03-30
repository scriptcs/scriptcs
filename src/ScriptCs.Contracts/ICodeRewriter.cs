namespace ScriptCs.Contracts
{
    public interface ICodeRewriter
    {
        FilePreProcessorResult Rewrite(FilePreProcessorResult code);
    }
}