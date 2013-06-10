using System;

namespace ScriptCs.Contracts
{
    public interface IConsole
    {
        void Write(string value);
        void WriteLine(string value);
        string ReadLine();
        void Exit();
        void ResetColor();
        ConsoleColor ForegroundColor { get; set; }
    }
}


