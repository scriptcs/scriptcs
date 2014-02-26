using System;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplBuffer : IReplBuffer
    {
        public string Line { get { return _buffer.ToString(); } set { ResetTo(0); Append(value); } }
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

        public void Back(int count = 1)
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

        public void Append(char ch)
        {
            _buffer.Insert(Position, ch);
            _console.Write(ch);
            Position++;
        }

        public void Append(string str)
        {
            _buffer.Insert(Position, str);
            _console.Write(str);
            Position += str.Length;
        }
    }
}
