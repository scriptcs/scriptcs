using System;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class InputLine : IInputLine
    {
        private readonly ILineStateMachine _lineStateMachine;

        public InputLine(ILineStateMachine lineStateMachine)
        {
            _lineStateMachine = lineStateMachine;
        }

        public string ReadLine(IConsole console, IScriptExecutor executor)
        {
            var buffer = new Buffer(console);
            bool isEOL = false;
            bool isCompletingWord = false;
            string[] possibleCompletions = null;
            int currentCompletion = 0;

            while (!isEOL)
            {
                var keyInfo = console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        isEOL = true;
                        console.WriteLine();
                        break;
                    case ConsoleKey.Backspace:
                        if (buffer.Position > 0)
                            buffer.Back();
                        break;
                    case ConsoleKey.Tab:
                        if (_lineStateMachine.CurrentState == LineState.FilePath)
                        {
                            if (!isCompletingWord)
                            {
                                var pathFragment = _lineStateMachine.CurrentName;
                                possibleCompletions = FindPossibleFilePaths(pathFragment);
                                currentCompletion = 0;
                                isCompletingWord = true;
                            }
                            else
                            {
                                currentCompletion = (currentCompletion + 1) % possibleCompletions.Length;
                            }

                            buffer.ResetTo(_lineStateMachine.StateStartPosition);
                            buffer.Append(possibleCompletions[currentCompletion]);
                        }
                        break;
                    default:
                        var ch = keyInfo.KeyChar;
                        buffer.Append(ch);
                        break;
                }

                _lineStateMachine.Consume(keyInfo);
            }

            return buffer.Line;
        }

        private string[] FindPossibleFilePaths(string pathFragment)
        {
            throw new NotImplementedException();
        }

        internal class Buffer
        {
            internal string Line { get { return buffer.ToString(); } }
            internal int Position { get { return buffer.Length; } }

            private readonly StringBuilder buffer = new StringBuilder();
            private readonly IConsole _console;

            internal Buffer(IConsole console)
            {
                _console = console;
            }

            internal void Back(int count = 1)
            {
                int steps = Math.Min(count, Position);

                if (steps > 0)
                {
                    buffer.Remove(Position - steps, steps);
                    foreach (int i in Enumerable.Range(1, steps))
                    {
                        _console.Write("\b \b");
                    }
                }
            }

            internal void ResetTo(int newPosition)
            {
                int stepCount = Position - newPosition;

                Back(stepCount);
            }

            internal void Append(char ch)
            {
                buffer.Append(ch);
                _console.Write(ch);
            }

            internal void Append(string str)
            {
                buffer.Append(str);
                _console.Write(str);
            }
        }
    }
}
