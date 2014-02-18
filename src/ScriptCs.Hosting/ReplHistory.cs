using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplHistory : IReplHistory
    {
        public string CurrentLine { get { return _history.ElementAt(_history.Count - _currentLine - 1); } }

        private const int HistorySize = 100;
        readonly Queue<string> _history = new Queue<string>(HistorySize); 
        int _currentLine = -1;

        public void AddLine(string line)
        {
            if (_history.Count == HistorySize) _history.Dequeue();
            _history.Enqueue(line);
            _currentLine = -1;
        }

        public string GetPreviousLine()
        {
            _currentLine = (_currentLine + 1) % _history.Count;

            return CurrentLine;
        }

        public string GetNextLine()
        {
            _currentLine = _currentLine <= 0 ? _history.Count - 1 : (_currentLine - 1) % _history.Count;

            return CurrentLine;
        }
    }
}