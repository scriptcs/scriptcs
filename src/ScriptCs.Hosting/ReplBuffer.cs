using System;
using System.Linq;
using System.Text;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplBuffer : IReplBuffer
    {
        public string Line { get { return _buffer.ToString(); } set { ResetTo(0); Append(value); } }
        public int Position { get { return _buffer.Length; } }

        private readonly StringBuilder _buffer = new StringBuilder();
        private IConsole _console;

        public ReplBuffer(IConsole console)
        {
            _console = console;
        }

        public void StartLine()
        {
            _buffer.Clear();
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
            }
        }

        public void ResetTo(int newPosition)
        {
            int stepCount = Position - newPosition;

            Back(stepCount);
        }

        public void Append(char ch)
        {
            _buffer.Append(ch);
            _console.Write(ch);
        }

        public void Append(string str)
        {
            _buffer.Append(str);
            _console.Write(str);
        }
    }
}
