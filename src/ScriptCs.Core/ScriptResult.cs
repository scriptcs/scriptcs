using System;

namespace ScriptCs
{
    public class ScriptResult
    {
        public object ReturnValue { get; set; }

        public Exception ExecuteException { get; set; }

        public Exception CompileException { get; set; }

        public bool ContinueBuffering { get; set; }

        public bool IsPendingClosingChar { get; set; }

        public char? ExpectingClosingChar { get; set; }

        public void UpdateClosingExpectation(Exception ex)
        {
            var message = ex.Message;
            char? closingChar = null;

            if (message.Contains("CS1026: ) expected"))
                closingChar = ')';
            else if (message.Contains("CS1513: } expected"))
                closingChar = '}';
            else if (message.Contains("CS1003: Syntax error, ']' expected"))
                closingChar = ']';

            ExpectingClosingChar = closingChar;
            IsPendingClosingChar = closingChar.HasValue;
        }
    }
}