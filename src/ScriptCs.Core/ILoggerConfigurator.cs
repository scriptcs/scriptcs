using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
    public interface ILoggerConfigurator
    {
        void Configure(IConsole console);
        
        Common.Logging.ILog GetLogger();
    }
}
