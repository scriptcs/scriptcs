using System;

namespace ScriptCs.Exceptions
{
    public class ScriptExecutionException : Exception
    {
        public ScriptExecutionException(string message) : base(message)
        {
        }
    }
}
