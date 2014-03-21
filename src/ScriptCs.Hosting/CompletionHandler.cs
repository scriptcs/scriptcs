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
        private string _pathFragment;

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
                _pathFragment = _lineAnalyzer.CurrentText;
                _possibleCompletions = getPaths(_pathFragment);
                _currentCompletion = 0;
                _isCompletingWord = true;
            }
            else if (_possibleCompletions.Any())
            {
                _currentCompletion = (_currentCompletion + 1) % _possibleCompletions.Length;
            }

            if (_possibleCompletions.Any())
            {
                UpdateBuffer();
            }
        }

        public void UpdateBufferWithPrevious()
        {
            if (_isCompletingWord && _possibleCompletions.Any())
            {
                _currentCompletion = _currentCompletion == 0
                    ? _possibleCompletions.Length - 1
                    : _currentCompletion - 1;

                UpdateBuffer();
            }
        }

        public void Abort()
        {
            if (_isCompletingWord)
            {
                UpdateBuffer(_pathFragment);
                Reset();
            }
        }

        public void Reset()
        {
            _isCompletingWord = false;
        }

        private void UpdateBuffer()
        {
            UpdateBuffer(_possibleCompletions[_currentCompletion]);
        }

        private void UpdateBuffer(string completion)
        {
            _buffer.ResetTo(_lineAnalyzer.TextPosition);
            _buffer.Insert(completion);
        }
    }
}