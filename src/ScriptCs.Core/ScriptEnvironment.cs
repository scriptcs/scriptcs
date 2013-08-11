using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ScriptCs
{
    public class ScriptEnvironment
    {
        public ScriptEnvironment() { }

        public ScriptEnvironment(
            IEnumerable<string> references,
            IEnumerable<string> namespaces,
            FileInfo rootScriptInfo,
            string[] scriptArgs,
            string script)
        {
            References = references;
            Namespaces = namespaces;
            RootScriptInfo = rootScriptInfo;
            ScriptArgs = scriptArgs;
            Script = script;
        }

        public IEnumerable<string> References { get; private set; }

        public IEnumerable<string> Namespaces { get; private set; }

        public FileInfo RootScriptInfo { get; private set; }

        public string[] ScriptArgs { get; private set; }

        public string Script { get; private set; }

        public Assembly Assembly { get; set; }

        public Exception LastException { get; set; }
    }
}