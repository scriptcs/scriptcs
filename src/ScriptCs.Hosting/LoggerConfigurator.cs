using ScriptCs.Contracts;
using ScriptCs.Logging;
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
            _logger = new ScriptConsoleLogger(_logLevel, console, LogProvider.GetLogger(LoggerName));
        }

        public void Configure(IConsole console, ILog log)
        {
            _logger = new ScriptConsoleLogger(_logLevel, console, log);
        }

        public ILog GetLogger()
        {
            return _logger;
        }
    }
}