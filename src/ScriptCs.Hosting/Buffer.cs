using System;
using System.Configuration;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class Buffer
    {
        internal string Line { get { return _buffer.ToString(); } set { ResetTo(0); Append(value); } }
        internal int Position { get { return _buffer.Length; } }

        private readonly StringBuilder _buffer = new StringBuilder();
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
                _buffer.Remove(Position - steps, steps);
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
            _buffer.Append(ch);
            _console.Write(ch);
        }

        internal void Append(string str)
        {
            _buffer.Append(str);
            _console.Write(str);
        }
    }
}
