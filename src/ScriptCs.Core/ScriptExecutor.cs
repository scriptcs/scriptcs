using System.Collections.Generic;
using System.IO;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptExecutor : IScriptExecutor
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFilePreProcessor _filePreProcessor;
        private readonly IScriptEngine _scriptEngine;

        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine)
        {
            _fileSystem = fileSystem;
            _filePreProcessor = filePreProcessor;
            _scriptEngine = scriptEngine;
        }

        public object Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            var bin = Path.Combine(_fileSystem.GetWorkingDirectory(script), "bin");
            var files = PrepareBinFolder(paths, bin);
    
            var references = new List<string>();
            references.Add("System");
            references.Add("System.Core");
            references.AddRange(files);

            _scriptEngine.BaseDirectory = bin;

            var scriptPackSession = new ScriptPackSession(scriptPacks);
            scriptPackSession.InitializePacks();

            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var code = _filePreProcessor.ProcessFile(path);
            
            var result = _scriptEngine.Execute(
                            code: code,
                            references: references,
                            scriptPackSession: scriptPackSession);

            scriptPackSession.TerminatePacks();

            return result;
        }

        private IEnumerable<string> PrepareBinFolder(IEnumerable<string> paths, string bin)
        {
            var files = new List<string>();

            if (!_fileSystem.DirectoryExists(bin))
                _fileSystem.CreateDirectory(bin);

            foreach (var file in paths)
            {
                var destFile = Path.Combine(bin, Path.GetFileName(file));
                var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(file);
                var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);
                if (sourceFileLastWriteTime != destFileLastWriteTime)
                    _fileSystem.Copy(file, destFile, true);
                files.Add(destFile);
            }

            return files;
        }
    }
}