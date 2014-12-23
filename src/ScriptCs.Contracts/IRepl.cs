namespace ScriptCs.Contracts
{
    using System.Collections.Generic;

    public interface IRepl : IScriptExecutor
    {
        Dictionary<string, IReplCommand> Commands { get; }

        string Buffer { get; }
    }
}
