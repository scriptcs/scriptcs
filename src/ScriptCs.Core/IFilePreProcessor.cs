using System.Collections.Generic;

namespace ScriptCs
{
    public interface IFilePreProcessor
    {
        FilePreProcessorResult ProcessFile(string path);

        FilePreProcessorResult ProcessScript(string script);
    }
}