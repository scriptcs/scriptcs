using System;
using ScriptCs.Contracts;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
    public class LoggerConfigurator : ILoggerConfigurator
    {
        private const string LoggerName = "scriptcs";

        private readonly LogLevel _logLevel;

        private Common.Logging.ILog _logger;

        public LoggerConfigurator(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Configure(IConsole console)
        {
            _logger = new ScriptConsoleLogger(_logLevel, console, Common.Logging.LogManager.GetLogger(LoggerName));
        }

        public Common.Logging.ILog GetLogger()
        {
            return _logger;
        }
    }
}
