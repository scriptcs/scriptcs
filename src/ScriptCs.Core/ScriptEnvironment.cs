using System.Collections.Generic;

namespace ScriptCs
{
    public class ScriptEnvironment
    {
        public ScriptEnvironment() { }

        public ScriptEnvironment(
            IEnumerable<string> references,
            string[] scriptArgs,
            string script,
            IEnumerable<string> namespaces)
        {
            References = references;
            ScriptArgs = scriptArgs;
            Script = script;
            Namespaces = namespaces;
        }

        public IEnumerable<string> References { get; private set; }

        public string[] ScriptArgs { get; private set; }

        public string Script { get; private set; }

        public IEnumerable<string> Namespaces { get; private set; }
    }
}