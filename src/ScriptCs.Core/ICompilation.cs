using System.IO;
using Roslyn.Compilers.Common;

namespace ScriptCs
{
    public interface ICompilation
    {
        CommonEmitResult Emit(Stream outputStream, Stream pdbStream);
    }
}