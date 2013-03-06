using System;
using Roslyn.Scripting.CSharp;
using ScriptCs.Contracts;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace ScriptCs
{
    public class ScriptExecutor : IScriptExecutor
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFilePreProcessor _filePreProcessor;

        [ImportingConstructor]
        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor)
        {
            _fileSystem = fileSystem;
            _filePreProcessor = filePreProcessor;
        }

        public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptCsRecipe> recipes)
        {
            var engine = new ScriptEngine();
            engine.AddReference("System");
            engine.AddReference("System.Core");
            var bin = Path.Combine(_fileSystem.CurrentDirectory, "bin");
            engine.BaseDirectory = bin;

            if (!_fileSystem.DirectoryExists(bin))
                _fileSystem.CreateDirectory(bin);

            foreach (var file in paths)
            {
                var destFile = Path.Combine(bin, Path.GetFileName(file));
                var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(file);
                var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);
                if (sourceFileLastWriteTime != destFileLastWriteTime)
                    _fileSystem.Copy(file, destFile, true);

                engine.AddReference(destFile);
            }

            var session = engine.CreateSession();
            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var csx = _filePreProcessor.ProcessFile(path);
            session.Execute(csx);
        }
    }
}