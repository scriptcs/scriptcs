using System.Collections.Generic;

namespace ScriptCs
{
    public class FilePreProcessorResult
    {
        public FilePreProcessorResult()
        {
            Usings = new List<string>();
            LoadedScripts = new List<string>();
            References = new List<string>();
        }

        public List<string> Usings { get; set; }

        public List<string> LoadedScripts { get; set; }

        public List<string> References { get; set; }

        public string Code { get; set; }
    }
}