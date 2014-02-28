using System;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class CompletionHandler : ICompletionHandler
    {
        private bool _isCompletingWord;
        private string[] _possibleCompletions;
        private int _currentCompletion;
        private readonly ILineAnalyzer _lineAnalyzer;
        private readonly IReplBuffer _buffer;

        public CompletionHandler(ILineAnalyzer lineAnalyzer, IReplBuffer buffer)
        {
            _buffer = buffer;
            _lineAnalyzer = lineAnalyzer;
            _isCompletingWord = false;
            _possibleCompletions = null;
            _currentCompletion = 0;
        }

        public void UpdateBufferWithCompletion(Func<string, string[]> getPaths)
        {
            if (!_isCompletingWord)
            {
                var pathFragment = _lineAnalyzer.CurrentText;
                _possibleCompletions = getPaths(pathFragment);
                _currentCompletion = 0;
                _isCompletingWord = true;
            }
            else if (_possibleCompletions.Any())
            {
                _currentCompletion = (_currentCompletion + 1) % _possibleCompletions.Length;
            }

            if (_possibleCompletions.Any())
            {
                _buffer.ResetTo(_lineAnalyzer.TextPosition);
                _buffer.Insert(_possibleCompletions[_currentCompletion]);
            }
        }

        public void Reset()
        {
            _isCompletingWord = false;
        }
    }
}