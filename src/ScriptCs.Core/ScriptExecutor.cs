using System.Collections.Generic;
using System.IO;
using System.Linq;

using log4net;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptExecutor : IScriptExecutor
    {
        private static readonly string[] DefaultReferences = new[] {"System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq"};
        private static readonly string[] DefaultNamespaces = new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks"};

        private readonly IFileSystem _fileSystem;
        private readonly IFilePreProcessor _filePreProcessor;
        private readonly IScriptEngine _scriptEngine;
        private readonly ILog _logger;

        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILog logger)
        {
            _fileSystem = fileSystem;
            _filePreProcessor = filePreProcessor;
            _scriptEngine = scriptEngine;
            _logger = logger;
        }

        public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            var bin = Path.Combine(_fileSystem.GetWorkingDirectory(script), "bin");
    
            var references = DefaultReferences.Union(paths);

            _scriptEngine.BaseDirectory = bin;

            _logger.Debug("Initializing script packs");
            var scriptPackSession = new ScriptPackSession(scriptPacks);
            scriptPackSession.InitializePacks();

            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var code = _filePreProcessor.ProcessFile(path);

            _scriptEngine.Execute(code, references, DefaultNamespaces, scriptPackSession);

            scriptPackSession.TerminatePacks();
        }

        private IEnumerable<string> PrepareBinFolder(IEnumerable<string> paths, string bin)
        {
            var files = new List<string>();

            if (!_fileSystem.DirectoryExists(bin))
            {
                _logger.DebugFormat("Creating directory {0}", bin);
                _fileSystem.CreateDirectory(bin);
            }

            foreach (var file in paths)
            {
                var destFile = Path.Combine(bin, Path.GetFileName(file));
                var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(file);
                var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);
                if (sourceFileLastWriteTime != destFileLastWriteTime)
                {
                    _logger.DebugFormat("Copying file {0} to bin folder {1}", file, destFile);
                    _fileSystem.Copy(file, destFile, true);
                }
                
                files.Add(destFile);
            }

            return files;
        }
    }
}