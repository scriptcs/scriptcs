using System;

namespace ScriptCs.Contracts.Logging
{
    public delegate bool Logger(
        LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
}
