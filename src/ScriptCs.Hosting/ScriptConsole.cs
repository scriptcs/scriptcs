using System;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ScriptConsole : IConsole
    {
        public ScriptConsole()
        {
            Initialize();
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void Write(char value)
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

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey(true);
        }

        public void Clear()
        {
            Console.Clear();
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

        public int HorizontalPosition
        {
            get { return Console.CursorLeft; }
            set { Console.CursorLeft = value; }
        }

        public int VerticalPosition
        {
            get { return Console.CursorTop; }
            set { Console.CursorTop = value; }
        }

        public int Width { get { return Console.BufferWidth; } }

        private void Initialize()
        {
            Console.CancelKeyPress += HandleCancelKeyPress;
        }
    }
}