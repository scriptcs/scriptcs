namespace ScriptCs.Contracts
{
    public interface IFilePreProcessor : IFileParser
    {
        FilePreProcessorResult ProcessFile(string path);

        FilePreProcessorResult ProcessScript(string script);
    }
}