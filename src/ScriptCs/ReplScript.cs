using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public class ReplScript
    {
        private readonly string NewLine;
        private readonly IScriptEngine ScriptEngine;
        private readonly ScriptPackSession ScriptPackSession;
        private IEnumerable<string> References;
        private IEnumerable<string> Namespaces; 

        private int _lastExecutedLineIndex = -1;

        public ReplScript(string newLine, IEnumerable<string> references, IEnumerable<string> namespaces, IScriptEngine scriptEngine, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("newLine", newLine);
            Guard.AgainstNullArgument("scriptEngine", scriptEngine);

            NewLine = newLine;
            ScriptEngine = scriptEngine;
            ScriptPackSession = scriptPackSession;
            References = references;
            Namespaces = namespaces;

            Lines = new List<string>();
        }

        public List<string> Lines { get; private set; }

        public string PendingLine { get; private set; }

        public ScriptExecutionResult LastResult { get; private set; }

        public void Append(string input)
        {
            var lines = (PendingLine + input).Split(new[] {NewLine}, StringSplitOptions.None);
            if (lines.Length > 1)
                throw new Exception("Can't add multiple lines of input");

            PendingLine = lines[0];
        }

        public bool RemoveInput(int charCount)
        {
            if (PendingLine == null)
                return false;
            if (PendingLine.Length < charCount)
                return false;
            PendingLine = PendingLine.Substring(0, PendingLine.Length - charCount);
            if (PendingLine.Length == 0)
                PendingLine = null;
            return true;
        }

        public ScriptExecutionResult Execute()
        {
            if (PendingLine != null)
            {
                Lines.Add(PendingLine);
                PendingLine = null;
            }

            string scriptChunk = string.Join(NewLine, Lines.Skip(_lastExecutedLineIndex + 1));

            var result = ScriptEngine.Execute(scriptChunk, References, Namespaces, ScriptPackSession);
            if (!result.ScriptIsMissingClosingChar.HasValue)
                _lastExecutedLineIndex = Lines.Count - 1;

            return result;
        }
    }
}
