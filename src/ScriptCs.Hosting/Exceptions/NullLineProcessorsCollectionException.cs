using System;
using System.Runtime.Serialization;

namespace ScriptCs.Hosting.Exceptions
{
    [Serializable]
    public class NullLineProcessorsCollectionException : Exception
    {
        public NullLineProcessorsCollectionException(string message)
            : base(message)
        {
        }

        public NullLineProcessorsCollectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NullLineProcessorsCollectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
