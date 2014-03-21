using System;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplBuffer : IReplBuffer
    {
        public string Line { get { return _buffer.ToString(); } set { ResetTo(0); Insert(value); } }
        public int Position { get; private set; }

        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly IConsole _console;
        private int _promptLength;
        private int _currentLineNumber;
        private int _verticalStartPosition;

        public ReplBuffer(IConsole console)
        {
            _console = console;
        }

        public void StartLine()
        {
            _buffer.Clear();
            Position = 0;
            _promptLength = _console.HorizontalPosition;
            _currentLineNumber = 0;
            _verticalStartPosition = _console.VerticalPosition;
        }

        public void Back() // Moq does not like optional parameters
        {
            Back(1);
        }

        public void Back(int count)
        {
            int steps = Math.Min(count, Position);

            if (steps > 0)
            {
                _buffer.Remove(Position - steps, steps);
                Position -= steps;
                SetConsolePosition();
                _console.Write(_buffer.ToString().Substring(Position).PadRight(steps + _buffer.Length - Position));
                SetConsolePosition();
            }
        }

        public void Delete()
        {
            if (Position < _buffer.Length)
            {
                _buffer.Remove(Position, 1);
                _console.Write(_buffer.ToString().Substring(Position) + " ");
                SetConsolePosition();
            }
        }

        public void MoveLeft()
        {
            if (Position > 0)
            {
                Position--;
                SetConsolePosition();
            }
        }

        public void MoveRight()
        {
            if (Position < _buffer.Length)
            {
                Position++;
                SetConsolePosition(); 
            }
        }

        public void ResetTo(int newPosition)
        {
            int stepCount = Position - newPosition;

            Back(stepCount);
        }

        public void Insert(char ch)
        {
            _console.Write(ch + _buffer.ToString().Substring(Position));
            _buffer.Insert(Position, ch);
            Position++;
            SetConsolePosition();
        }
        
        public void Insert(string str)
        {
            _console.Write(str + _buffer.ToString().Substring(Position));
            _buffer.Insert(Position, str);
            Position += str.Length;
            SetConsolePosition();
        }

        private void SetConsolePosition()
        {
            var newPos = Position + _promptLength;
            _currentLineNumber = newPos / _console.Width;
            
            _console.HorizontalPosition = newPos % _console.Width;
            var newVerticalPosition = _verticalStartPosition + _currentLineNumber;
            if (_console.VerticalPosition != newVerticalPosition) // Minimizes console updates and keeps tests simpler
            {
                _console.VerticalPosition = newVerticalPosition;
            }
        }
    }
}
