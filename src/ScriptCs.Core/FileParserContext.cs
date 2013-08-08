using System.Collections.Generic;

namespace ScriptCs
{
    public class FileParserContext
    {
        public FileParserContext()
        {
            Namespaces = new List<string>();
            References = new List<string>();
            LoadedScripts = new List<string>();
            Body = new List<string>();
        }

        public List<string> Namespaces { get; private set; }

        public List<string> References { get; private set; }

        public List<string> LoadedScripts { get; private set; }

        public List<string> Body { get; private set; }
    }
}