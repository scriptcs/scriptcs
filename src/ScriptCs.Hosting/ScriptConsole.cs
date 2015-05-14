using System;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ScriptConsole : IConsole
    {
        public ScriptConsole()
        {
            Console.CancelKeyPress += HandleCancelKeyPress;
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void Exit()
        {
            ResetColor();
            Console.CancelKeyPress -= HandleCancelKeyPress;
            Environment.Exit(0);
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

        public int Width
        {
            get { return Console.BufferWidth; }
        }
    }
}