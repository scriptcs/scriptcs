using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public ScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine)
        {
            _fileSystem = fileSystem;
            _filePreProcessor = filePreProcessor;
            _scriptEngine = scriptEngine;
        }

        public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks)
        {
            var bin = Path.Combine(_fileSystem.GetWorkingDirectory(script), "bin");

            var references = DefaultReferences.Union(paths);

            _scriptEngine.BaseDirectory = bin;

            var scriptPackSession = new ScriptPackSession(scriptPacks);
            scriptPackSession.InitializePacks();

            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var result = _filePreProcessor.ProcessFile(path);

            _scriptEngine.Execute(result.Code, references.Union(result.References), DefaultNamespaces, scriptPackSession);

            scriptPackSession.TerminatePacks();
        }
    }
}