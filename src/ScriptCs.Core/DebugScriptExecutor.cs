using System.IO;
using ScriptCs.Exceptions;

namespace ScriptCs
{
    public class DebugScriptExecutor : ScriptExecutor
    {
        private readonly ICompiledDllDebugger _compiledDllDebugger;

        public DebugScriptExecutor(
            IFileSystem fileSystem, 
            IFilePreProcessor filePreProcessor, 
            IScriptEngine scriptEngine, 
            ICompiledDllDebugger compiledDllDebugger,
            IScriptHostFactory scriptHostFactory)
            : base(fileSystem, filePreProcessor, scriptEngine, scriptHostFactory)
        {
            _compiledDllDebugger = compiledDllDebugger;
        }

        public DebugScriptExecutor(
            IFileSystem fileSystem,
            IFilePreProcessor filePreProcessor,
            IScriptEngine scriptEngine,
            ICompiledDllDebugger compiledDllDebugger)
            : this(fileSystem, filePreProcessor, scriptEngine, compiledDllDebugger, new ScriptHostFactory())
        {
        }

        protected override void Execute(string absolutePathToScript, ISession session, string code)
        {
            var fileName = Path.GetFileName(absolutePathToScript);
            var nameWithoutExtension = fileName.Replace(Path.GetExtension(fileName), string.Empty);
            var outputName = nameWithoutExtension + ".dll";
            var pdbName = nameWithoutExtension + ".pdb";
            var outputPath = Path.Combine(session.Engine.BaseDirectory, outputName);
            var pdbPath = Path.Combine(session.Engine.BaseDirectory, pdbName);
            
            ISubmission<object> submission = session.CompileSubmission<object>(code);

            ICompilationResult result;

            using (Stream outputStream = _fileSystem.CreateFileStream(outputPath, FileMode.OpenOrCreate))
            using (Stream pdbStream = _fileSystem.CreateFileStream(pdbPath, FileMode.OpenOrCreate))
            {
                 result = submission.Compilation.Emit(outputStream, pdbStream);
            }

            if (result.Success)
            {
                _compiledDllDebugger.Run(outputPath, session);   
            }
            else
            {
                throw new CompilationException(result.ErrorMessage);
            }
        }
    }
}
