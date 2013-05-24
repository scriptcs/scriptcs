using System;

namespace ScriptCs
{
    public class ScriptResult
    {
        public object ReturnValue { get; set; }

        public Exception ExecuteException { get; set; }

        public Exception CompileException { get; set; }
    }
}