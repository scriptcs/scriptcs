namespace ScriptCs.Contracts
{
    using System;

    public delegate bool Logger(
        LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
}
