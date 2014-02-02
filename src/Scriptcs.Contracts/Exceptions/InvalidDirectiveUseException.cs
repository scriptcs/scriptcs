using System;
using System.Runtime.Serialization;

namespace ScriptCs.Contracts.Exceptions
{
    [Serializable]
    public class InvalidDirectiveUseException : Exception
    {
        public InvalidDirectiveUseException(string message)
            : base(message)
        {
        }

        public InvalidDirectiveUseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidDirectiveUseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
