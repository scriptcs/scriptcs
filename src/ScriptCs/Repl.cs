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
        private readonly IConsole _console;
        private ScriptPackSession _scriptPackSession;
        private IEnumerable<string> _references; 

        public Repl(IFileSystem fileSystem, IScriptEngine scriptEngine, ILog logger, IConsole console)
        {
            _fileSystem = fileSystem;
            _scriptEngine = scriptEngine;
            _logger = logger;
            _console = console;
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
            var foregroundColor = _console.ForegroundColor;

            try
            {
                _console.ForegroundColor = ConsoleColor.Cyan;
                _scriptEngine.Execute(script, _references, DefaultNamespaces, _scriptPackSession);
            }
            catch (Exception ex)
            {
                _console.ForegroundColor = ConsoleColor.Red;
                _console.WriteLine("\r\n" + ex + "\r\n");
            }
            _console.ForegroundColor = foregroundColor;
        }
    }
}
