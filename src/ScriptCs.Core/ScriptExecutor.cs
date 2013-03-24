﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            var bin = Path.Combine(_fileSystem.GetWorkingDirectory(script), "bin");

            var references = new List<string> { "System", "System.Core" }.Union(paths);

            _scriptEngine.BaseDirectory = bin;

            var scriptPackSession = new ScriptPackSession(scriptPacks);
            scriptPackSession.InitializePacks();

            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var code = _filePreProcessor.ProcessFile(path);

            _scriptEngine.Execute(code, references, scriptPackSession);

            scriptPackSession.TerminatePacks();
        }
    }
}