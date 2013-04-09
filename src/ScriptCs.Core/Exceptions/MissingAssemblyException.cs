using System;

namespace ScriptCs.Exceptions
{
    public class MissingAssemblyException : Exception
    {
        public MissingAssemblyException(string message) : base(message)
        {
            
        }
    }
}
