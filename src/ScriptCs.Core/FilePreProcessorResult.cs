using System.Collections.Generic;

namespace ScriptCs
{
    public class FilePreProcessorResult
    {
        public FilePreProcessorResult()
        {
            UsingStatements = new List<string>();
            LoadedScripts = new List<string>();
            References = new List<string>();
        }

        public List<string> UsingStatements { get; set; }

        public List<string> LoadedScripts { get; set; }

        public List<string> References { get; set; }

        public string Code { get; set; }
    }
}