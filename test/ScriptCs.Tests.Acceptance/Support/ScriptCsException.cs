using System;
using System.Runtime.Serialization;

namespace ScriptCs.Tests.Acceptance.Support
{
    [Serializable]
    public class ScriptCsException : Exception
    {
        public ScriptCsException()
        {
        }

        public ScriptCsException(string message)
            : base(message)
        {
        }

        public ScriptCsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ScriptCsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
