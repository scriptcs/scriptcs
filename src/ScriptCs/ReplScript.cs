using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public class ReplScript
    {
        private readonly string _newLine;
        private readonly IScriptEngine _scriptEngine;
        private readonly ScriptPackSession _scriptPackSession;
        private readonly IEnumerable<string> _references;
        private readonly IEnumerable<string> Namespaces; 

        private int _lastExecutedLineIndex = -1;

        public ReplScript(string newLine, IEnumerable<string> references, IEnumerable<string> namespaces, IScriptEngine scriptEngine, ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("newLine", newLine);
            Guard.AgainstNullArgument("scriptEngine", scriptEngine);

            _newLine = newLine;
            _scriptEngine = scriptEngine;
            _scriptPackSession = scriptPackSession;
            _references = references;
            Namespaces = namespaces;

            Lines = new List<string>();
        }

        public List<string> Lines { get; private set; }

        public string PendingLine { get; private set; }

        public ScriptExecutionResult LastResult { get; private set; }

        public char? MissingClosingChar { get; set; }

        public void Append(string input)
        {
            var lines = (PendingLine + input).Split(new[] {_newLine}, StringSplitOptions.None);
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
            return true;
        }

        public ScriptExecutionResult Execute()
        {
            if (PendingLine != null)
            {
                Lines.Add(PendingLine);
                PendingLine = null;
            }

            string scriptChunk = string.Join(_newLine, Lines.Skip(_lastExecutedLineIndex + 1));

            var result = _scriptEngine.Execute(scriptChunk, _references, Namespaces, _scriptPackSession);
            if (!result.ScriptIsMissingClosingChar.HasValue)
                _lastExecutedLineIndex = Lines.Count - 1;

            return result;
        }

        public void EmptyLine()
        {
            PendingLine = "";
        }
    }
}
