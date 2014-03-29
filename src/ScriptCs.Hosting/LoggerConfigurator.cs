using Common.Logging;
using ScriptCs.Contracts;
using ILog = Common.Logging.ILog;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    public class LoggerConfigurator : ILoggerConfigurator
    {
        private const string LoggerName = "scriptcs";

        private readonly LogLevel _logLevel;

        private ILog _logger;

        public LoggerConfigurator(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Configure(IConsole console)
        {
            _logger = new ScriptConsoleLogger(_logLevel, console, LogManager.GetLogger(LoggerName));
        }

        public ILog GetLogger()
        {
            return _logger;
        }
    }
}