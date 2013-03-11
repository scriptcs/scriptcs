using System.IO;

namespace ScriptCs
{
    public interface ICompilation
    {
        ICompilationResult Emit(Stream outputStream, Stream pdbStream);
    }
}