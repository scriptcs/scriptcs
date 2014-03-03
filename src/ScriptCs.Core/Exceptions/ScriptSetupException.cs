using System;

namespace ScriptCs.Exceptions
{
    [Serializable]
    public class ScriptSetupException : Exception
    {
        public ScriptSetupException(string message)
            : base(message)
        {
        }

        public ScriptSetupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}