using System;
using System.Runtime.ExceptionServices;

namespace ScriptCs.Contracts
{
    public class ScriptResult
    {
        public ScriptResult() { }

        public object ReturnValue { get; private set; }

        public ExceptionDispatchInfo ExecuteExceptionInfo { get; private set; }

        public ExceptionDispatchInfo CompileExceptionInfo { get; private set; }

        public bool IsCompleteSubmission { get; private set; }

        public static ScriptResult IncompleteSubmission
        {
            get { return new ScriptResult { IsCompleteSubmission = false }; }
        }

        public static ScriptResult FromReturnValue(object returnValue)
        {
            return new ScriptResult
            {
                ReturnValue = returnValue,
                IsCompleteSubmission = true
            };
        }

        public static ScriptResult FromExecutionException(Exception exception)
        {
            return new ScriptResult
            {
                ExecuteExceptionInfo = ExceptionDispatchInfo.Capture(exception),
                IsCompleteSubmission = true
            };
        }

        public static ScriptResult FromCompilationException(Exception exception)
        {
            return new ScriptResult
            {
                CompileExceptionInfo = ExceptionDispatchInfo.Capture(exception),
                IsCompleteSubmission = true
            };
        }
    }
}