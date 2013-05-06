using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Repl 
    {
        private static readonly string[] DefaultReferences = new[] { "System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq" };
        private static readonly string[] DefaultNamespaces = new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks" };

        private readonly IFileSystem _fileSystem;
        private readonly IScriptEngine _scriptEngine;
        private readonly ILog _logger;
        private ScriptPackSession _scriptPackSession;
        private IEnumerable<string> _references; 

        public Repl(IFileSystem fileSystem, IScriptEngine scriptEngine, ILog logger)
        {
            _fileSystem = fileSystem;
            _scriptEngine = scriptEngine;
            _logger = logger;
        }

        public void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            _references = DefaultReferences.Union(paths);
            var bin = Path.Combine(_fileSystem.CurrentDirectory, "bin");

            _scriptEngine.BaseDirectory = bin;

            _logger.Debug("Initializing script packs");
            var scriptPackSession = new ScriptPackSession(scriptPacks);

            scriptPackSession.InitializePacks();
            _scriptPackSession = scriptPackSession;

        }

        public void Terminate()
        {
            _logger.Debug("Terminating packs");
            _scriptPackSession.TerminatePacks();
        }

        public void Execute(string script)
        {
            _scriptEngine.Execute(script, _references, DefaultNamespaces, _scriptPackSession);
        }
    }
}
