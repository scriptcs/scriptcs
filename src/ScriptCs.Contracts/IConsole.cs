using System;

namespace ScriptCs.Contracts
{
    public interface IConsole
    {
        void Write(string value);

        void Write(char value);

        void WriteLine();

        void WriteLine(string value);

        string ReadLine();

        ConsoleKeyInfo ReadKey();

        void Clear();

        void Exit();

        void ResetColor();

        ConsoleColor ForegroundColor { get; set; }
    }
}