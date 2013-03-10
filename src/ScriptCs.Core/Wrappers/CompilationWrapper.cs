using Roslyn.Compilers.Common;

namespace ScriptCs.Wrappers
{
    using System.IO;

    public class CompilationWrapper : ICompilation
    {
        private readonly CommonCompilation _compilation;

        public CompilationWrapper(CommonCompilation compilation)
        {
            this._compilation = compilation;
        }

        public ICompilationResult Emit(Stream outputStream, Stream pdbStream)
        {
            var emitResult = this._compilation.Emit(outputStream, pdbStream: pdbStream);
            return new CompilationResult(emitResult);
        }
    }
}