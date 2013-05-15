using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public class ScriptExecutionResult
    {
        public Exception CompilationException { get; set; }

        public object RuntimeException { get; set; }

        public object Result { get; set; }

        public char? ScriptIsMissingClosingChar { get; set; }

        public void UpdateClosingExpectation(Exception ex)
        {
            var message = ex.Message;
            char? closingChar = null;

            if (message.Contains("CS1026: ) expected"))
                closingChar = ')';
            else if (message.Contains("CS1513: } expected"))
                closingChar = '}';
            else if (message.Contains("CS1002: ; expected"))
                closingChar = ';';
            else if (message.Contains("CS1003: ] expected"))
                closingChar = ']';

            if (closingChar.HasValue)
                ScriptIsMissingClosingChar = closingChar.Value;
        }
    }
}
