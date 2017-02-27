using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public class FileParserContext
    {
        public FileParserContext()
        {
            Namespaces = new List<string>();
            AssemblyReferences = new List<string>();
            LoadedScripts = new List<string>();
            BodyLines = new List<string>();
            CustomReferences = new List<string>();
        }

        public List<string> Namespaces { get; private set; }

        public List<string> AssemblyReferences { get; private set; }

        public List<string> CustomReferences { get; private set; } 

        public List<string> LoadedScripts { get; private set; }

        public List<string> BodyLines { get; private set; }

        public string ScriptPath { get; set; }
    }
}