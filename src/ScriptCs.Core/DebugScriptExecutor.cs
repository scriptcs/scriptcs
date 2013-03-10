namespace ScriptCs
{
    using System.ComponentModel.Composition;
    using System.IO;

    using Roslyn.Compilers.Common;

    using ScriptCs.Exceptions;

    [Export(Constants.DebugContractName, typeof(IScriptExecutor))]
    public class DebugScriptExecutor : ScriptExecutor
    {
        [ImportingConstructor]
        public DebugScriptExecutor(IFileSystem fileSystem, [Import(Constants.DebugContractName)]IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, IScriptHostFactory scriptHostFactory)
            : base(fileSystem, filePreProcessor, scriptEngine, scriptHostFactory)
        {
        }

        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine)
            : base(fileSystem, filePreProcessor, scriptEngine)
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

            }
            else
            {
                throw new CompilationException(result.ErrorMessage);
            }
        }
    }
}
