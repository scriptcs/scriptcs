using System;
using System.Runtime.Serialization;

namespace ScriptCs.Exceptions
{
    [Serializable]
    public class MissingAssemblyException : Exception
    {
        public MissingAssemblyException(string message)
            : base(message)
        {
        }

        public MissingAssemblyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MissingAssemblyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
