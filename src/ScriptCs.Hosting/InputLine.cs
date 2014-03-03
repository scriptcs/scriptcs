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

        private enum Token { Backspace, Tab, BackTab, Escape, Delete, Enter, UpArrow, DownArrow, LeftArrow, RightArrow, Other}
        private readonly IReplHistory _replHistory;
        private readonly IReplBuffer _buffer;
        private readonly IConsole _console;
        private readonly IFilePathFinder _filePathFinder;
        private readonly ICompletionHandler _completionHandler;

        public InputLine(ILineAnalyzer lineAnalyzer, IReplHistory replHistory, IReplBuffer buffer, IConsole console, IFilePathFinder filePathFinder, ICompletionHandler completionHandler)
        {
            _completionHandler = completionHandler;
            _console = console;
            _buffer = buffer;
            _lineAnalyzer = lineAnalyzer;
            _replHistory = replHistory;
            _filePathFinder = filePathFinder;
        }

        public string ReadLine()
        {
            bool isEol = false;

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
                        _completionHandler.Reset();
                        break;
                    case Token.UpArrow:
                        var previousLine = _replHistory.GetPreviousLine();
                        if (previousLine != null) _buffer.Line = previousLine;
                        _completionHandler.Reset();
                        break;
                    case Token.DownArrow:
                        var nextLine = _replHistory.GetNextLine();
                        if (nextLine != null) _buffer.Line = nextLine;
                        _completionHandler.Reset();
                        break;
                    case Token.LeftArrow:
                        _buffer.MoveLeft();
                        _completionHandler.Reset();
                        break;
                    case Token.RightArrow:
                        _buffer.MoveRight();
                        _completionHandler.Reset();
                        break;
                    case Token.Backspace:
                        if (_buffer.Position > 0)
                            _buffer.Back();
                        _completionHandler.Reset();
                        break;
                    case Token.Delete:
                        if (_buffer.Position < _buffer.Line.Length)
                            _buffer.Delete();
                        _completionHandler.Reset();
                        break;
                    case Token.Tab:
                        if (_lineAnalyzer.CurrentState == LineState.FilePath)
                        {
                            _completionHandler.UpdateBufferWithCompletion(pathFragment => _filePathFinder.FindPossibleFilePaths(pathFragment));
                        }
                        else if (_lineAnalyzer.CurrentState == LineState.AssemblyName)
                        {
                            _completionHandler.UpdateBufferWithCompletion(nameFragment => _filePathFinder.FindPossibleAssemblyNames(nameFragment));
                        }
                        break;
                    case Token.BackTab:
                        _completionHandler.UpdateBufferWithPrevious();
                        break;
                    case Token.Escape:
                        _completionHandler.Abort();
                        break;
                    case Token.Other:
                        var ch = keyInfo.KeyChar;
                        _buffer.Insert(ch);
                        _completionHandler.Reset();
                        break;
                }

                _lineAnalyzer.Analyze(_buffer.Line);
            }

            if (_buffer.Line.Length > 0) _replHistory.AddLine(_buffer.Line);

            return _buffer.Line;
        }
        
        private Token Tokenize(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Tab)
            {
                if (keyInfo.Modifiers == ConsoleModifiers.Shift) return Token.BackTab;

                return Token.Tab;
            }
            if (keyInfo.Key == ConsoleKey.Enter) return Token.Enter;
            if (keyInfo.Key == ConsoleKey.Escape) return Token.Escape;
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
