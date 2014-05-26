using System;
using System.Runtime.ExceptionServices;

namespace ScriptCs.Contracts
{
    public class ScriptResult
    {
        public static readonly ScriptResult Empty = new ScriptResult();

        public static readonly ScriptResult Incomplete = new ScriptResult { IsCompleteSubmission = false };

        public ScriptResult()
        {
            // Explicit default ctor to use as mock return value.
            IsCompleteSubmission = true;
        }

        public ScriptResult(
            object returnValue = null,
            Exception executionException = null,
            Exception compilationException = null)
        {
            if (returnValue != null)
            {
                ReturnValue = returnValue;
            }

            if (executionException != null)
            {
                ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(executionException);
            }

            if (compilationException != null)
            {
                CompileExceptionInfo = ExceptionDispatchInfo.Capture(compilationException);
            }

            IsCompleteSubmission = true;
        }

        public object ReturnValue { get; private set; }

        public ExceptionDispatchInfo ExecuteExceptionInfo { get; private set; }

        public ExceptionDispatchInfo CompileExceptionInfo { get; private set; }

        public bool IsCompleteSubmission { get; private set; }
    }
}