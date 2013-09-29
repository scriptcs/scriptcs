using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptExecutor
    {
        void ImportNamespaces(params string[] namespaces);

        void AddReferences(params string[] references);

        void RemoveReferences(params string[] references);

        void RemoveNamespaces(params string[] namespaces);

        void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks, params string[] scriptArgs);

        void Terminate();

        ScriptResult Execute(string script, params string[] scriptArgs);

        ScriptResult ExecuteScript(string script, params string[] scriptArgs);
    }
}
