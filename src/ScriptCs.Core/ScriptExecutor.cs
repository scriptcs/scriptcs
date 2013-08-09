using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptExecutor : IScriptExecutor
    {
        public static readonly string[] DefaultReferences = new[] { "System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq" };
        public static readonly string[] DefaultNamespaces = new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "System.IO" };

        public IFileSystem FileSystem { get; private set; }
        public IFilePreProcessor FilePreProcessor { get; private set; }
        public IScriptEngine ScriptEngine { get; private set; }
        public ILog Logger { get; private set; }
        public Collection<string> References { get; private set; }
        public Collection<string> Namespaces { get; private set; }
        public ScriptPackSession ScriptPackSession { get; protected set; }

        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILog logger)
        {
            References = new Collection<string>();
            AddReferences(DefaultReferences);
            Namespaces = new Collection<string>();
            AddNamespaces(DefaultNamespaces);
            FileSystem = fileSystem;
            FilePreProcessor = filePreProcessor;
            ScriptEngine = scriptEngine;
            Logger = logger;
        }

        public void AddNamespaces(IEnumerable<string> namespaces)
        {
            Guard.AgainstNullArgument("namespaces", namespaces);

            foreach(var @namespace in namespaces)
            {
                AddNamespace(@namespace);
            }
        }

        public void AddNamespace(string @namespace)
        {
            Guard.AgainstNullArgument("namespace", @namespace);

            Namespaces.Add(@namespace);
        }

        public void AddNamespaceByType(Type typeFromReferencedAssembly)
        {
            Guard.AgainstNullArgument("typeFromReferencedAssembly", typeFromReferencedAssembly);

            AddNamespace(typeFromReferencedAssembly.Namespace);
        }

        public void AddNamespaceByType<T>()
        {
            AddNamespaceByType(typeof(T));
        }

        public void AddReferences(IEnumerable<string> paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            foreach(var path in paths)
            {
                AddReference(path);
            }
        }

        public void AddReferenceByType(Type typeFromReferencedAssembly)
        {
            Guard.AgainstNullArgument("typeFromReferencedAssembly", typeFromReferencedAssembly);

            AddReference(typeFromReferencedAssembly.Assembly.Location);
        }

        public void AddReferenceByType<T>()
        {
            AddReferenceByType(typeof(T));
        }

        public void AddReference(string path)
        {
            Guard.AgainstNullArgument("path", path);

            References.Add(path);
        }

        public void RemoveReference(string path)
        {
            Guard.AgainstNullArgument("path", path);

            References.Remove(path);
        }

        public virtual void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            AddReferences(paths);
            var bin = Path.Combine(FileSystem.CurrentDirectory, "bin");

            ScriptEngine.BaseDirectory = bin;

            Logger.Debug("Initializing script packs");
            var scriptPackSession = new ScriptPackSession(scriptPacks);

            scriptPackSession.InitializePacks();
            ScriptPackSession = scriptPackSession;
        }

        public virtual void Terminate()
        {
            Logger.Debug("Terminating packs");
            ScriptPackSession.TerminatePacks();
        }

        public virtual ScriptResult Execute(string script)
        {
            return Execute(script, new string[0]);
        }

        public virtual ScriptResult Execute(string script, string[] scriptArgs)
        {
            var path = Path.IsPathRooted(script) ? script : Path.Combine(FileSystem.CurrentDirectory, script);
            var result = FilePreProcessor.ProcessFile(path);
            var references = References.Union(result.References);
            var namespaces = Namespaces.Union(result.Namespaces);

            Logger.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, namespaces, ScriptPackSession);
        }
    }
}