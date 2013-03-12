using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptPackSession
    {
        List<string> References { get; }
        List<string> Namespaces { get; }

        void AddReference(string assemblyDisplayNameOrPath);
        void ImportNamespace(string @namespace);
    }
}
