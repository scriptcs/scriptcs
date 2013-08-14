using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public class FileParserContext
    {
        public FileParserContext()
        {
            Namespaces = new List<string>();
            References = new List<string>();
            LoadedScripts = new List<string>();
            BodyLines = new List<string>();
        }

        public List<string> Namespaces { get; private set; }

        public List<string> References { get; private set; }

        public List<string> LoadedScripts { get; private set; }

        public List<string> BodyLines { get; private set; }
    }
}