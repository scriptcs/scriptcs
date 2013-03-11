using System;

namespace ScriptCs.Exceptions
{
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {
        }
    }
}
