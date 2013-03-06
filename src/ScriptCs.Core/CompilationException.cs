using System;
using Roslyn.Compilers;

namespace ScriptCs
{
    public class CompilationException : Exception
    {
        public string Path { get; private set; }
        public CompilationException(string path, CompilationErrorException roslynException)
            : base(string.Format("{0}: {1}", path, roslynException.Message))
        {
            Path = path;
        }
    }
}