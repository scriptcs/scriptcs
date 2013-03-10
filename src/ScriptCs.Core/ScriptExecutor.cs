using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptExecutor : IScriptExecutor
    {
        protected readonly IFileSystem _fileSystem;
        private readonly IFilePreProcessor _filePreProcessor;
        private readonly IScriptEngine _scriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;

        [ImportingConstructor]
        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, IScriptHostFactory scriptHostFactory)
        {
            _fileSystem = fileSystem;
            _filePreProcessor = filePreProcessor;
            _scriptEngine = scriptEngine;
            _scriptHostFactory = scriptHostFactory;
        }

        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine) :
            this(fileSystem, filePreProcessor, scriptEngine, new ScriptHostFactory())
        {

        }

        public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            this._scriptEngine.AddReference("System");
            this._scriptEngine.AddReference("System.Core");

            var bin = Path.Combine(_fileSystem.GetWorkingDirectory(script), "bin");

            _scriptEngine.BaseDirectory = bin;

            var files = PrepareBinFolder(paths, bin);
            var contexts = GetContexts(scriptPacks);
            var host = _scriptHostFactory.CreateScriptHost(contexts);
            var session = _scriptEngine.CreateSession(host);
            AddReferences(files, session);
            var scriptPackSession = new ScriptPackSession(session);
            InitializeScriptPacks(scriptPacks, scriptPackSession);
            var absolutePathToScript = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var processedCode = _filePreProcessor.ProcessFile(absolutePathToScript);
            Execute(absolutePathToScript, session, processedCode);
            TerminateScriptPacks(scriptPacks);
        }

        protected virtual void Execute(string absolutePathToScript, ISession session, string code)
        {
            session.Execute(code);
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

        private void AddReferences(IEnumerable<string> files, ISession session)
        {
            foreach (var file in files)
            {
                session.AddReference(file);
            }
        }

        private IEnumerable<IScriptPackContext> GetContexts(IEnumerable<IScriptPack> scriptPacks)
        {
            foreach (var pack in scriptPacks)
            {
                yield return pack.GetContext();
            }
        }

        private void InitializeScriptPacks(IEnumerable<IScriptPack> scriptPacks, IScriptPackSession session)
        {
            foreach (var pack in scriptPacks)
            {
                pack.Initialize(session);
            }
        }

        private void TerminateScriptPacks(IEnumerable<IScriptPack> scriptPacks)
        {
            foreach (var pack in scriptPacks)
            {
                pack.Terminate();
            }
        }

    }
}