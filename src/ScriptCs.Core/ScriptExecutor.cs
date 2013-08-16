using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public abstract class ScriptExecutor : IScriptExecutor
    {
        public static readonly string[] DefaultReferences =
        {
            "System", 
            "System.Xml", 
            "System.Core", 
            "System.Data", 
            "System.Xml.Linq",
            "System.Data.DataSetExtensions" 
        };

        public static readonly string[] DefaultNamespaces =
        {
            "System", 
            "System.IO",
            "System.Linq", 
            "System.Text", 
            "System.Threading.Tasks", 
            "System.Collections.Generic" 
        };

        protected ScriptExecutor(
            IFileSystem fileSystem,
            IFilePreProcessor filePreProcessor,
            IScriptEngine scriptEngine,
            ILog logger)
        {
            References = new List<string>(DefaultReferences);
            Namespaces = new List<string>(DefaultNamespaces);
            FileSystem = fileSystem;
            FilePreProcessor = filePreProcessor;
            ScriptEngine = scriptEngine;
            Logger = logger;
        }

        public IFileSystem FileSystem { get; private set; }

        public IFilePreProcessor FilePreProcessor { get; private set; }

        public IScriptEngine ScriptEngine { get; private set; }

        public ILog Logger { get; private set; }

        public List<string> References { get; private set; }

        public List<string> Namespaces { get; private set; }

        public ScriptPackSession ScriptPackSession { get; protected set; }

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

            foreach (var path in paths)
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

            ScriptEngine.BaseDirectory = Path.Combine(FileSystem.CurrentDirectory, "bin");

            Logger.Debug("Initializing script packs");
            ScriptPackSession = new ScriptPackSession(scriptPacks);

            ScriptPackSession.InitializePacks();
        }

        public virtual void Terminate()
        {
            Logger.Debug("Terminating packs");
            ScriptPackSession.TerminatePacks();
        }

        public virtual ScriptResult ExecuteFile(string path, params string[] scriptArgs)
        {
            var rootedPath = Path.IsPathRooted(path) ? path : Path.Combine(FileSystem.CurrentDirectory, path);
            var code = FileSystem.ReadFile(rootedPath);

            ScriptEngine.FileName = Path.GetFileName(rootedPath);

            return ExecuteCode(code, scriptArgs);
        }

        public virtual ScriptResult ExecuteCode(string code, params string[] scriptArgs)
        {
            var result = FilePreProcessor.ProcessCode(code);

            var references = References.Union(result.References);
            var namespaces = Namespaces.Union(result.Namespaces);

            Logger.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, namespaces, ScriptPackSession);
        }
    }
}