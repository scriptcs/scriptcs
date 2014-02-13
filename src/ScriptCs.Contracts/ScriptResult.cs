using System;
using System.Runtime.ExceptionServices;

namespace ScriptCs.Contracts
{
    public class ScriptResult
    {
        public object ReturnValue { get; set; }

        public ExceptionDispatchInfo ExecuteExceptionInfo { get; set; }

        public ExceptionDispatchInfo CompileExceptionInfo { get; set; }

        public bool IsPendingClosingChar { get; set; }

        public char? ExpectingClosingChar { get; set; }

        public void UpdateClosingExpectation(Exception ex)
        {
            Guard.AgainstNullArgument("ex", ex);

            var message = ex.Message;
            char? closingChar = null;

            if (message.Contains("CS1026: ) expected"))
            {
                closingChar = ')';
            }
            else if (message.Contains("CS1513: } expected"))
            {
                closingChar = '}';
            }
            else if (message.Contains("CS1003: Syntax error, ']' expected"))
            {
                closingChar = ']';
            }

            ExpectingClosingChar = closingChar;
            IsPendingClosingChar = closingChar.HasValue;
        }

        public IScriptHost ScriptHost { get; private set; }
    }
}