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
        private readonly char[] SLASHES = {'\\', '/'};
        private enum Token { Backspace, Tab, Delete, Enter, UpArrow, DownArrow, LeftArrow, RightArrow, Other}
        private readonly IReplHistory _replHistory;
        private readonly IReplBuffer _buffer;
        private readonly IConsole _console;

        public InputLine(ILineAnalyzer lineAnalyzer, IReplHistory replHistory, IReplBuffer buffer, IConsole console)
        {
            _console = console;
            _buffer = buffer;
            _lineAnalyzer = lineAnalyzer;
            _replHistory = replHistory;
        }

        public string ReadLine(IScriptExecutor executor)
        {
            bool isEol = false;
            bool isCompletingWord = false;
            string[] possibleCompletions = null;
            int currentCompletion = 0;

            _buffer.StartLine();

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
                        break;
                    case Token.DownArrow:
                        _buffer.Line = _replHistory.GetNextLine();
                        break;
                    case Token.Backspace:
                        if (_buffer.Position > 0)
                            _buffer.Back();
                        isCompletingWord = false;
                        break;
                    case Token.Tab:
                        if (_lineAnalyzer.CurrentState == LineState.FilePath)
                        {
                            if (!isCompletingWord)
                            {
                                var pathFragment = _lineAnalyzer.CurrentText;
                                possibleCompletions = FindPossibleFilePaths(pathFragment, executor.FileSystem);
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
                                _buffer.Append(possibleCompletions[currentCompletion]); 
                            }
                        }
                        else if (_lineAnalyzer.CurrentState == LineState.AssemblyName)
                        {
                            if (!isCompletingWord)
                            {
                                var nameFragment = _lineAnalyzer.CurrentText;
                                possibleCompletions = FindPossibleAssemblyNames(nameFragment, executor.FileSystem);
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
                                _buffer.Append(possibleCompletions[currentCompletion]);
                            }
                        }
                        break;
                    case Token.Other:
                        var ch = keyInfo.KeyChar;
                        _buffer.Append(ch);
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

        private string[] FindPossibleAssemblyNames(string nameFragment, IFileSystem fileSystem)
        {
            var roots = new List<string>
            {
                fileSystem.CurrentDirectory,                
            };

            AddGACRoots(@"C:\Windows\Microsoft.Net\assembly", roots, fileSystem);

            return FindPossiblePaths(nameFragment, roots.Distinct(), fileSystem);
        }

        private void AddGACRoots(string node, List<string> roots, IFileSystem fileSystem)
        {
            if (fileSystem.EnumerateFiles(node, "*.dll", SearchOption.TopDirectoryOnly).Any()) roots.Add(node);

            var subDirs = fileSystem.EnumerateDirectories(node, "*", SearchOption.TopDirectoryOnly);

            foreach (var dir in subDirs)
            {
                AddGACRoots(dir, roots, fileSystem);
            }
       
        }

        private string[] FindPossibleFilePaths(string pathFragment, IFileSystem fileSystem)
        {
            return FindPossiblePaths(pathFragment, new[] { fileSystem.CurrentDirectory }, fileSystem);
        }

        private string[] FindPossiblePaths(string pathFragment, IEnumerable<string> roots, IFileSystem fileSystem)
        {
            int lastSlashIndex = pathFragment.LastIndexOfAny(SLASHES);

            var possiblePaths = new List<string>();

            foreach (var r in roots)
            {
                string path;
                string pattern;
                var partialPath = pathFragment.Substring(0, lastSlashIndex + 1); 

                if (lastSlashIndex >= 0)
                {
                    path = Path.Combine(r, partialPath);
                    pattern = pathFragment.Substring(lastSlashIndex + 1);
                }
                else
                {
                    path = r;
                    pattern = pathFragment;
                }

                try
                {
                    possiblePaths.AddRange(fileSystem.EnumerateFilesAndDirectories(
                        path,
                        pattern + "*",
                        SearchOption.TopDirectoryOnly).Select(p => AugmentPathFragment(partialPath, pattern, p)));
                }
                catch (Exception)
                {
                    //
                }
            }

            return possiblePaths.Any() ? possiblePaths.ToArray() : new[] { pathFragment };
        }

        private string AugmentPathFragment(string partialPath, string nameFragment, string completePath)
        {
            int lastSlashIndex = completePath.LastIndexOfAny(SLASHES);
            var name = completePath.Substring(lastSlashIndex + 1);

            return Path.Combine(partialPath, name);
        }
    }
}
