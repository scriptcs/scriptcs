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
            ImportNamespaces(DefaultNamespaces);
            FileSystem = fileSystem;
            FilePreProcessor = filePreProcessor;
            ScriptEngine = scriptEngine;
            Logger = logger;
        }

        public void ImportNamespaces(params string[] namespaces)
        {
            Guard.AgainstNullArgument("namespaces", namespaces);

            foreach (var @namespace in namespaces)
            {
                Namespaces.Add(@namespace);
            }
        }


        public void AddReferences(params string[] paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            foreach(var path in paths)
            {
                References.Add(path);
            }
        }

        public void RemoveReferences(params string[] paths)
        {
            Guard.AgainstNullArgument("paths", paths);
            
            foreach (var path in paths)
            {
                References.Remove(path);
            }
        }

        public void RemoveNamespaces(params string[] namespaces)
        {
            Guard.AgainstNullArgument("namespaces", namespaces);

            foreach (var @namespace in namespaces)
            {
                Namespaces.Remove(@namespace);
            }
        }

        public virtual void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            AddReferences(paths.ToArray());
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

        public virtual ScriptResult Execute(string script, params string[] scriptArgs)
        {
            var path = Path.IsPathRooted(script) ? script : Path.Combine(FileSystem.CurrentDirectory, script);
            var result = FilePreProcessor.ProcessFile(path);
            var references = References.Union(result.References);
            var namespaces = Namespaces.Union(result.Namespaces);

            Logger.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, namespaces, ScriptPackSession);
        }

        public virtual ScriptResult ExecuteScript(string script, params string[] scriptArgs)
        {
            var result = FilePreProcessor.ProcessScript(script);
            var references = References.Union(result.References);
            var namespaces = Namespaces.Union(result.Namespaces);

            Logger.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, namespaces, ScriptPackSession);
        }
    }
}