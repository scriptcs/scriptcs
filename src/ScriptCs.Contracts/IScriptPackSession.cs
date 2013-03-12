using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptPackSession
    {
        void AddReference(string assemblyDisplayNameOrPath);
        void ImportNamespace(string @namespace);
    }
}
