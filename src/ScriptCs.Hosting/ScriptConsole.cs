using System;
using ScriptCs.Contracts;
using Mono.Terminal;

namespace ScriptCs.Hosting
{
    public class ScriptConsole : IConsole
    {
		LineEditor le;
        public ScriptConsole()
        {
			le = new LineEditor ("scriptcs");
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

		public string ReadLine(string prompt)
        {
			return le.Edit (prompt, "");
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void Exit()
        {
            ResetColor();
            Environment.Exit(0);
        }

        public void ResetColor()
        {
            Console.ResetColor();
        }

        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }
    }
}