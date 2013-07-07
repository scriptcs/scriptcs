using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public IEnumerable<string> References { get; protected set; }
        public ScriptPackSession ScriptPackSession { get; protected set; }

        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILog logger)
        {
            FileSystem = fileSystem;
            FilePreProcessor = filePreProcessor;
            ScriptEngine = scriptEngine;
            Logger = logger;
        }

        public virtual void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            References = DefaultReferences.Union(paths);
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
            string path;
            Uri uri;
            if (Uri.TryCreate(script, UriKind.Absolute, out uri))
            {
                path = Path.GetTempFileName();
                var request = WebRequest.Create(uri);

                Logger.DebugFormat("Downloading script from {0}", uri.ToString());
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var fileStream = File.OpenWrite(path))
                {
                    Logger.DebugFormat("Writing to temporary script file {0}", path);
                    responseStream.CopyTo(fileStream);
                }
            }
            else
            {
                path = Path.IsPathRooted(script) ? script : Path.Combine(FileSystem.CurrentDirectory, script);
            }

            var result = FilePreProcessor.ProcessFile(path);
            var references = References.Union(result.References);

            Logger.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, DefaultNamespaces, ScriptPackSession);
        }
    }
}