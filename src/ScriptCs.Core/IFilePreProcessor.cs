namespace ScriptCs
{
    public interface IFilePreProcessor
    {
        FilePreProcessingResult ProcessFile(string path);
    }
}