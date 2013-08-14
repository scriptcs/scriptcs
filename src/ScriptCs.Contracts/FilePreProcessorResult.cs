using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public class FilePreProcessorResult
    {
        public FilePreProcessorResult()
        {
            Namespaces = new List<string>();
            LoadedScripts = new List<string>();
            References = new List<string>();
        }

        public List<string> Namespaces { get; set; }

        public List<string> LoadedScripts { get; set; }

        public List<string> References { get; set; }

        public string Code { get; set; }
    }
}