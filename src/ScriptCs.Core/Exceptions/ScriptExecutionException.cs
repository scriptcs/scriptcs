using System;
using System.Runtime.Serialization;

namespace ScriptCs.Exceptions
{
    [Serializable]
    public class ScriptExecutionException : Exception
    {
        public ScriptExecutionException(string message)
            : base(message)
        {
        }

        public ScriptExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ScriptExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
