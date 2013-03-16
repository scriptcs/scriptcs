using System.Collections.Generic;
using System.IO;
using log4net;
using ScriptCs.Contracts;

namespace ScriptCs
{
    using System;

    public class ScriptExecutor : IScriptExecutor
    {
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
            var files = PrepareBinFolder(paths, bin);
    
            var references = new List<string>();
            _logger.Debug("Adding System reference");
            references.Add("System");
            _logger.Debug("Adding System.Core");
            references.Add("System.Core");
            _logger.DebugFormat("Adding references to files {0}", string.Join(Environment.NewLine, files));
            references.AddRange(files);

            _scriptEngine.BaseDirectory = bin;

            _logger.Debug("Initializing script packs");
            var scriptPackSession = new ScriptPackSession(scriptPacks);
            scriptPackSession.InitializePacks();

            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var code = _filePreProcessor.ProcessFile(path);
            
            _scriptEngine.Execute(
                code: code,
                references: references,
                scriptPackSession: scriptPackSession);

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