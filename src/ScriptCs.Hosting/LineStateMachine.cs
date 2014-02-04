using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class LineStateMachine : ILineStateMachine
    {
        public string CurrentName { get; private set; }
        public LineState CurrentState { get; private set; }
        public int StateStartPosition { get; private set; }

        public LineStateMachine()
        {
            CurrentName = null;
            CurrentState = LineState.Unknown;
            StateStartPosition = 0;
        }

        public void Consume(ConsoleKeyInfo keyInfo)
        {
            // TODO
        }
    }
}
