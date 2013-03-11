using System.IO;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;

namespace ScriptCs.Wrappers
{
    public class CompilationWrapper : ICompilation
    {
        private readonly CommonCompilation _compilation;

        public CompilationWrapper(Compilation compilation)
        {
            _compilation = compilation.UpdateOptions(
                    compilation.Options.WithDebugInformationKind(DebugInformationKind.Full)
                        .WithOptimizations(false));
        }

        public ICompilationResult Emit(Stream outputStream, Stream pdbStream)
        {
            var emitResult = _compilation.Emit(outputStream, pdbStream: pdbStream);
            return new CompilationResult(emitResult);
        }
    }
}