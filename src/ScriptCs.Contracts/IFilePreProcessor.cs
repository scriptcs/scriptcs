using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IFilePreProcessor
    {
        FilePreProcessorResult ProcessFile(string path);

        FilePreProcessorResult ProcessScript(string script);
    }
}