using System;
using System.Runtime.Serialization;

namespace ScriptCs.Exceptions
{
    [Serializable]
    public class ScriptCompilationException : Exception
    {
        public ScriptCompilationException(string message)
            : base(message)
        {
        }

        public ScriptCompilationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ScriptCompilationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}