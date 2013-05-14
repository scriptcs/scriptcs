namespace ScriptCs
{
    public interface IFilePreProcessor
    {
        FilePreProcessorResult ProcessFile(string path);
    }
}