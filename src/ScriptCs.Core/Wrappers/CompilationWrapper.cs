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

        public CommonEmitResult Emit(Stream outputStream, Stream pdbStream)
        {
            return this._compilation.Emit(outputStream, pdbStream: pdbStream);
        }
    }
}