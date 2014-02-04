using System;

namespace ScriptCs.Contracts
{
    public interface IInputLine
    {
        string ReadLine(IConsole console, IScriptExecutor executor);
    }
}
