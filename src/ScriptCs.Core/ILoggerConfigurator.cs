using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface ILoggerConfigurator
    {
        void Configure(IConsole console);
    }
}
