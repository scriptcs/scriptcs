using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptExecutor
    {
        void ImportNamespaces(params string[] namespaces);

        void AddReferences(params string[] references);

        void RemoveReferences(params string[] references);

        void RemoveNamespaces(params string[] namespaces);

        void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks);

        void Terminate();

        ScriptResult ExecuteFile(string path, params string[] scriptArgs);

        ScriptResult ExecuteCode(string code, params string[] scriptArgs);
    }
}