﻿namespace ScriptCs.Contracts
{
    public interface IReplBuffer
    {
        string Line { get; set; }
        int Position { get; }
        void StartLine();
        void Back(int count);
        void Back();
        void Delete();
        void MoveLeft();
        void MoveRight();
        void ResetTo(int newPosition);
        void Insert(char ch);
        void Insert(string str);
    }
}