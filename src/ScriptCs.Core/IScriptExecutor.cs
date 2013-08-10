using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
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
        void AddNamespaces(IEnumerable<string> namespaces);
        void AddNamespace(string @namespace);
        void AddNamespaceByType(Type typeFromReferencedAssembly);
        void AddNamespaceByType<T>();
        void AddReferences(IEnumerable<string> paths);
        void AddReferenceByType(Type typeFromReferencedAssembly);
        void AddReferenceByType<T>();
        void AddReference(string path);
        void RemoveReference(string path);
        void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks);
        void Terminate();
        ScriptResult Execute(string script);
        ScriptResult Execute(string script, string[] scriptArgs);
    }
}