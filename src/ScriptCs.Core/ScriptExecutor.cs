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

        public bool CacheAssembly
        {
            get { return _scriptEngine.CacheAssembly; }
            set { _scriptEngine.CacheAssembly = value; }
        }

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
            _scriptEngine.AssemblyName = Path.GetFileNameWithoutExtension(script);

            var path = Path.IsPathRooted(script) ? script : Path.Combine(_fileSystem.CurrentDirectory, script);
            var code = _filePreProcessor.ProcessFile(path);

            if (CacheAssembly)
            {
                var fileInfo = new FileInfo(path);
                _scriptEngine.AssemblyCacheDate = fileInfo.LastWriteTime;
            }

            var scriptPackSession = new ScriptPackSession(scriptPacks);

            scriptPackSession.InitializePacks();

            if (_scriptEngine.CanExecuteCached())
                _scriptEngine.ExecuteCached(references, DefaultNamespaces, scriptPackSession);
            else
                _scriptEngine.Execute(code, references, DefaultNamespaces, scriptPackSession);

            scriptPackSession.TerminatePacks();
        }
    }
}
