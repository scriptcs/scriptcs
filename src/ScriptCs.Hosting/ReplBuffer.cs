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

        public ReplBuffer(IConsole console)
        {
            _console = console;
        }

        public void StartLine()
        {
            _buffer.Clear();
            Position = 0;
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
                foreach (int i in Enumerable.Range(1, steps))
                {
                    _console.Write("\b \b");
                }
                Position -= steps;
            }
        }

        public void Delete()
        {
            if (Position < _buffer.Length)
            {
                _buffer.Remove(Position, 1);
                _console.Write(_buffer.ToString().Substring(Position) + " ");
            }
        }

        public void MoveLeft()
        {
            if (Position > 0)
            {
                Position--;
                _console.Position--;
            }
        }

        public void MoveRight()
        {
            if (Position < _buffer.Length)
            {
                Position++;
                _console.Position++; 
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
        }

        public void Insert(string str)
        {
            _console.Write(str + _buffer.ToString().Substring(Position));
            _buffer.Insert(Position, str);
            Position += str.Length;
        }
    }
}
