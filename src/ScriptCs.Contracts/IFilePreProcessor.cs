using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IFilePreProcessor : IFileParser
    {
        Task<FilePreProcessorResult> ProcessFile(string path);

        Task<FilePreProcessorResult> ProcessScript(string script);
    }
}