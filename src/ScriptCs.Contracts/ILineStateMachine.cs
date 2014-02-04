using System;

namespace ScriptCs.Contracts
{
    public interface ILineStateMachine
    {
        string CurrentName { get; }
        LineState CurrentState { get; }
        int StateStartPosition { get; }

        void Consume(ConsoleKeyInfo keyInfo);
    }

    public enum LineState { FilePath, AssemblyName, Identifier, Member, Unknown }
}
