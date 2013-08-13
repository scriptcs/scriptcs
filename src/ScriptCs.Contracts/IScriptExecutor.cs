using System.Collections.Generic;
using System.Collections.ObjectModel;

using Common.Logging;

namespace ScriptCs.Contracts
{
    public interface IScriptExecutor
    {
        IFileSystem FileSystem { get; }

        IFilePreProcessor FilePreProcessor { get; }

        IScriptEngine ScriptEngine { get; }

        ILog Logger { get; }

        Collection<string> References { get; }

        Collection<string> Namespaces { get; }

        ScriptPackSession ScriptPackSession { get; }

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
