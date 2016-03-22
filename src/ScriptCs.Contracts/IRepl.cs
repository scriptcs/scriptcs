namespace ScriptCs.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IRepl : IScriptExecutor
    {
        Dictionary<string, IReplCommand> Commands { get; }
        void AddCustomPrinter<T>(Func<T, string> printer);
        string Buffer { get; }
    }
}
