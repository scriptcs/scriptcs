using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplHistory : IReplHistory
    {
        public string CurrentLine { get { return _history.Count > 0 ? _history.ElementAt(_history.Count - _currentLine - 1) : String.Empty; } }

        private readonly int _limit;
        readonly Queue<string> _history; 
        int _currentLine = 0;
        private bool _justAddedLine = false;

        public ReplHistory(int limit = 100)
        {
            _limit = limit;
            _history = new Queue<string>(_limit);
        }

        public void AddLine(string line)
        {
            if (_history.Count == _limit) _history.Dequeue();
            _history.Enqueue(line);
            _currentLine = 0;
            _justAddedLine = true;
        }

        public string GetPreviousLine()
        {
            if (_history.Count == 0) return String.Empty;

            if (!_justAddedLine)
                _currentLine = (_currentLine + 1) % _history.Count;

            _justAddedLine = false;

            return CurrentLine;
        }

        public string GetNextLine()
        {
            if (_history.Count == 0) return String.Empty;

            _currentLine = _currentLine == 0 ? _history.Count - 1 : _currentLine - 1;

            _justAddedLine = false;

            return CurrentLine;
        }
    }
}