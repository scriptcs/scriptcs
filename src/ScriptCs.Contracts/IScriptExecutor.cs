using System.Collections.Generic;
using System.Reflection;

namespace ScriptCs.Contracts
{
    public interface IScriptExecutor
    {
        AssemblyReferences References { get; }

        IReadOnlyCollection<string> Namespaces { get; }

        IScriptEngine ScriptEngine { get; }

        IFileSystem FileSystem { get; }

        ScriptPackSession ScriptPackSession { get; }

        void ImportNamespaces(params string[] namespaces);

        void RemoveNamespaces(params string[] namespaces);

        void AddReferences(params Assembly[] references);

        void RemoveReferences(params Assembly[] references);

        void AddReferences(params string[] references);

        void RemoveReferences(params string[] references);

        void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks, params string[] scriptArgs);

        void Reset();

        void Terminate();

        ScriptResult Execute(string script, params string[] scriptArgs);

        ScriptResult ExecuteScript(string script, params string[] scriptArgs);
    }
}
