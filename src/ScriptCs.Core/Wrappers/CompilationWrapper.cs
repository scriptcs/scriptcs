using Roslyn.Compilers.Common;

namespace ScriptCs.Wrappers
{
    using System.IO;

    using Roslyn.Compilers.CSharp;

    public class CompilationWrapper : ICompilation
    {
        private readonly CommonCompilation _compilation;

        public CompilationWrapper(Compilation compilation)
        {
            this._compilation = compilation.UpdateOptions(
                    compilation.Options.WithDebugInformationKind(DebugInformationKind.Full)
                        .WithOptimizations(false));
        }

        public ICompilationResult Emit(Stream outputStream, Stream pdbStream)
        {
            var emitResult = this._compilation.Emit(outputStream, pdbStream: pdbStream);
            return new CompilationResult(emitResult);
        }
    }
}