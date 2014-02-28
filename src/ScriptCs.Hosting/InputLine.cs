using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class InputLine : IInputLine
    {
        private readonly ILineAnalyzer _lineAnalyzer;

        private enum Token { Backspace, Tab, Delete, Enter, UpArrow, DownArrow, LeftArrow, RightArrow, Other}
        private readonly IReplHistory _replHistory;
        private readonly IReplBuffer _buffer;
        private readonly IConsole _console;
        private readonly IFilePathFinder _filePathFinder;

        public InputLine(ILineAnalyzer lineAnalyzer, IReplHistory replHistory, IReplBuffer buffer, IConsole console, IFilePathFinder filePathFinder)
        {
            _console = console;
            _buffer = buffer;
            _lineAnalyzer = lineAnalyzer;
            _replHistory = replHistory;
            _filePathFinder = filePathFinder;
        }

        public string ReadLine(IScriptExecutor executor)
        {
            bool isEol = false;
            bool isCompletingWord = false;
            string[] possibleCompletions = null;
            int currentCompletion = 0;

            _buffer.StartLine();
            _lineAnalyzer.Reset();

            while (!isEol)
            {
                var keyInfo = _console.ReadKey();

                switch (Tokenize(keyInfo))
                {
                    case Token.Enter:
                        isEol = true;
                        _console.WriteLine();
                        isCompletingWord = false;
                        break;
                    case Token.UpArrow:
                        _buffer.Line = _replHistory.GetPreviousLine();
                        isCompletingWord = false;
                        break;
                    case Token.DownArrow:
                        _buffer.Line = _replHistory.GetNextLine();
                        isCompletingWord = false;
                        break;
                    case Token.LeftArrow:
                        _buffer.MoveLeft();
                        isCompletingWord = false;
                        break;
                    case Token.RightArrow:
                        _buffer.MoveRight();
                        isCompletingWord = false;
                        break;
                    case Token.Backspace:
                        if (_buffer.Position > 0)
                            _buffer.Back();
                        isCompletingWord = false;
                        break;
                    case Token.Delete:
                        if (_buffer.Position < _buffer.Line.Length)
                            _buffer.Delete();
                        isCompletingWord = false;
                        break;
                    case Token.Tab:
                        if (_lineAnalyzer.CurrentState == LineState.FilePath)
                        {
                            if (!isCompletingWord)
                            {
                                var pathFragment = _lineAnalyzer.CurrentText;
                                possibleCompletions = _filePathFinder.FindPossibleFilePaths(pathFragment, executor.FileSystem);
                                currentCompletion = 0;
                                isCompletingWord = true;
                            }
                            else if (possibleCompletions.Any())
                            {
                                currentCompletion = (currentCompletion + 1) % possibleCompletions.Length;
                            }

                            if (possibleCompletions.Any())
                            {
                                _buffer.ResetTo(_lineAnalyzer.TextPosition);
                                _buffer.Insert(possibleCompletions[currentCompletion]); 
                            }
                        }
                        else if (_lineAnalyzer.CurrentState == LineState.AssemblyName)
                        {
                            if (!isCompletingWord)
                            {
                                var nameFragment = _lineAnalyzer.CurrentText;
                                possibleCompletions = _filePathFinder.FindPossibleAssemblyNames(nameFragment, executor.FileSystem);
                                currentCompletion = 0;
                                isCompletingWord = true;
                            }
                            else if (possibleCompletions.Any())
                            {
                                currentCompletion = (currentCompletion + 1) % possibleCompletions.Length;
                            }

                            if (possibleCompletions.Any())
                            {
                                _buffer.ResetTo(_lineAnalyzer.TextPosition);
                                _buffer.Insert(possibleCompletions[currentCompletion]);
                            }
                        }
                        break;
                    case Token.Other:
                        var ch = keyInfo.KeyChar;
                        _buffer.Insert(ch);
                        isCompletingWord = false;
                        break;
                }

                _lineAnalyzer.Analyze(_buffer.Line);
            }

            if (_buffer.Line.Length > 0) _replHistory.AddLine(_buffer.Line);

            return _buffer.Line;
        }
        
        private Token Tokenize(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Tab) return Token.Tab;
            if (keyInfo.Key == ConsoleKey.Enter) return Token.Enter;
            if (keyInfo.Key == ConsoleKey.Backspace) return Token.Backspace;
            if (keyInfo.Key == ConsoleKey.Delete) return Token.Delete;
            if (keyInfo.Key == ConsoleKey.LeftArrow) return Token.LeftArrow;
            if (keyInfo.Key == ConsoleKey.UpArrow) return Token.UpArrow;
            if (keyInfo.Key == ConsoleKey.DownArrow) return Token.DownArrow;
            if (keyInfo.Key == ConsoleKey.RightArrow) return Token.RightArrow;

            return Token.Other;
        }
    }
}
