using System;
using System.Runtime.Serialization;

namespace ScriptCs.Exceptions
{
    [Serializable]
    public class ScriptPackException : Exception
    {
        public ScriptPackException(string message)
            : base(message)
        {
        }

        protected ScriptPackException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
