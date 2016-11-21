using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptExecutor : IScriptExecutor
    {
        private readonly ILog _log;

        public static readonly string[] DefaultReferences =
        {
            "System", 
            "System.Core", 
            "System.Data", 
            "System.Data.DataSetExtensions", 
            "System.Xml", 
            "System.Xml.Linq", 
            "System.Net.Http",
            "Microsoft.CSharp",
            typeof(ScriptExecutor).Assembly.Location,
            typeof(IScriptEnvironment).Assembly.Location
        };

        public static readonly string[] DefaultNamespaces =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq", 
            "System.Text", 
            "System.Threading.Tasks",
            "System.IO",
            "System.Net.Http",
            "System.Dynamic"
        };

        private const string ScriptLibrariesInjected = "ScriptLibrariesInjected";

        public IFileSystem FileSystem { get; private set; }

        public IFilePreProcessor FilePreProcessor { get; private set; }

        public IScriptEngine ScriptEngine { get; private set; }

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public Common.Logging.ILog Logger { get; private set; }

        public AssemblyReferences References { get; private set; }

        public IReadOnlyCollection<string> Namespaces { get; private set; }

        public ScriptPackSession ScriptPackSession { get; protected set; }

        public IScriptLibraryComposer ScriptLibraryComposer { get; protected set; }

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public ScriptExecutor(
            IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, Common.Logging.ILog logger)
            : this(fileSystem, filePreProcessor, scriptEngine, new CommonLoggingLogProvider(logger), new NullScriptLibraryComposer())
        {
        }

        public ScriptExecutor(
            IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILogProvider logProvider)
            : this(fileSystem, filePreProcessor, scriptEngine, logProvider, new NullScriptLibraryComposer())
        {
        }

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public ScriptExecutor(
            IFileSystem fileSystem,
            IFilePreProcessor filePreProcessor,
            IScriptEngine scriptEngine,
            Common.Logging.ILog logger,
            IScriptLibraryComposer composer)
            : this(fileSystem, filePreProcessor, scriptEngine, new CommonLoggingLogProvider(logger), composer)
        {
        }

        public ScriptExecutor(
            IFileSystem fileSystem,
            IFilePreProcessor filePreProcessor,
            IScriptEngine scriptEngine,
            ILogProvider logProvider,
            IScriptLibraryComposer composer)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgumentProperty("fileSystem", "BinFolder", fileSystem.BinFolder);
            Guard.AgainstNullArgumentProperty("fileSystem", "DllCacheFolder", fileSystem.DllCacheFolder);
            Guard.AgainstNullArgument("filePreProcessor", filePreProcessor);
            Guard.AgainstNullArgument("scriptEngine", scriptEngine);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("composer", composer);

            References = new AssemblyReferences(DefaultReferences);
            Namespaces = new ReadOnlyCollection<string>(DefaultNamespaces);
            FileSystem = fileSystem;
            FilePreProcessor = filePreProcessor;
            ScriptEngine = scriptEngine;
            _log = logProvider.ForCurrentType();
#pragma warning disable 618
            Logger = new ScriptCsLogger(_log);
#pragma warning restore 618
            ScriptLibraryComposer = composer;
        }

        public virtual void ImportNamespaces(params string[] namespaces)
        {
            Guard.AgainstNullArgument("namespaces", namespaces);
            Namespaces = new ReadOnlyCollection<string>(Namespaces.Union(namespaces).ToArray());
        }

        public virtual void RemoveNamespaces(params string[] namespaces)
        {
            Guard.AgainstNullArgument("namespaces", namespaces);
            Namespaces = new ReadOnlyCollection<string>(Namespaces.Except(namespaces).ToArray());
        }

        public virtual void AddReferences(params Assembly[] assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            References = References.Union(assemblies);
        }

        public virtual void RemoveReferences(params Assembly[] assemblies)
        {
            Guard.AgainstNullArgument("assemblies", assemblies);

            References = References.Except(assemblies);
        }

        public virtual void AddReferences(params string[] paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            References = References.Union(paths);
        }

        public virtual void RemoveReferences(params string[] paths)
        {
            Guard.AgainstNullArgument("paths", paths);

            References = References.Except(paths);
        }

        public virtual void Initialize(
            IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks, params string[] scriptArgs)
        {
            AddReferences(paths.ToArray());
            var bin = Path.Combine(FileSystem.CurrentDirectory, FileSystem.BinFolder);
            var cache = Path.Combine(FileSystem.CurrentDirectory, FileSystem.DllCacheFolder);

            ScriptEngine.BaseDirectory = bin;
            ScriptEngine.CacheDirectory = cache;

            _log.Debug("Initializing script packs");
            var scriptPackSession = new ScriptPackSession(scriptPacks, scriptArgs);
            ScriptPackSession = scriptPackSession;
            scriptPackSession.InitializePacks();
        }

        public virtual void Reset()
        {
            References = new AssemblyReferences(DefaultReferences);
            Namespaces = new ReadOnlyCollection<string>(DefaultNamespaces);
            ScriptPackSession.State.Clear();
        }

        public virtual void Terminate()
        {
            _log.Debug("Terminating packs");
            ScriptPackSession.TerminatePacks();
        }

        public virtual ScriptResult Execute(string script, params string[] scriptArgs)
        {
            var path = Path.IsPathRooted(script) ? script : Path.Combine(FileSystem.CurrentDirectory, script);
            var result = FilePreProcessor.ProcessFile(path);
            ScriptEngine.FileName = Path.GetFileName(path);
            return EngineExecute(Path.GetDirectoryName(path), scriptArgs, result);
        }

        public virtual ScriptResult ExecuteScript(string script, params string[] scriptArgs)
        {
            var result = FilePreProcessor.ProcessScript(script);
            return EngineExecute(FileSystem.CurrentDirectory, scriptArgs, result);
        }

        protected internal virtual ScriptResult EngineExecute(
            string workingDirectory, 
            string[] scriptArgs, 
            FilePreProcessorResult result
        )
        {
            InjectScriptLibraries(workingDirectory, result, ScriptPackSession.State);
            var namespaces = Namespaces.Union(result.Namespaces);
            var references = References.Union(result.References);
            _log.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, namespaces, ScriptPackSession);
        }

        protected internal virtual void InjectScriptLibraries(
            string workingDirectory,
            FilePreProcessorResult result,
            IDictionary<string, object> state
        )
        {
            Guard.AgainstNullArgument("result", result);
            Guard.AgainstNullArgument("state", state);

            if (state.ContainsKey(ScriptLibrariesInjected))
            {
                return;
            }

            var scriptLibrariesPreProcessorResult = LoadScriptLibraries(workingDirectory);

            if (scriptLibrariesPreProcessorResult != null)
            {
                result.Code = scriptLibrariesPreProcessorResult.Code + Environment.NewLine + result.Code;
                result.References.AddRange(scriptLibrariesPreProcessorResult.References);
                result.Namespaces.AddRange(scriptLibrariesPreProcessorResult.Namespaces);
            }
            state.Add(ScriptLibrariesInjected, null);
        }

        protected internal virtual FilePreProcessorResult LoadScriptLibraries(string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(ScriptLibraryComposer.ScriptLibrariesFile))
            {
                return null;
            }

            var scriptLibrariesPath = Path.Combine(workingDirectory, FileSystem.PackagesFolder,
                ScriptLibraryComposer.ScriptLibrariesFile);

            if (FileSystem.FileExists(scriptLibrariesPath))
            {
                _log.DebugFormat("Found Script Library at {0}", scriptLibrariesPath);
                return FilePreProcessor.ProcessFile(scriptLibrariesPath);
            }

            return null;
        }
    }
}
