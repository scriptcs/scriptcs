using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ReplConsole : IConsole
    {
        public ReplConsole()
        {
            Console.CancelKeyPress += HandleCancelKeyPress;
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void Exit()
        {
            ResetColor();
            Console.CancelKeyPress -= HandleCancelKeyPress;
        }

        public void ResetColor()
        {
            Console.ResetColor();
        }

        private void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ResetColor();
        }

        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }
    }
}