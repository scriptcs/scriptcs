using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

using ICommonLog = Common.Logging.ILog;

using Common.Logging.Log4Net;

namespace ScriptCs
{
    public class LoggerConfigurator
    {
        private const string Pattern = "%-5level Thread[%thread]: %message%newline";
        private const string LoggerName = "scriptcs";

        private readonly string _logLevel;

        private ICommonLog _logger;

        public LoggerConfigurator(string logLevel)
        {
            _logLevel = logLevel;
        }

        public void Configure()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = LogManager.GetLogger(LoggerName);
            var consoleAppender = new ConsoleAppender
            {
                Layout = new PatternLayout(Pattern),
                Threshold = hierarchy.LevelMap[_logLevel]
            };

            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            _logger = new CodeConfigurableLog4NetLogger(logger);
        }

        public ICommonLog GetLogger()
        {
            return _logger;
        }

        private class CodeConfigurableLog4NetLogger : Log4NetLogger
        {
            protected internal CodeConfigurableLog4NetLogger(ILoggerWrapper log)
                : base(log)
            {
            }
        }
    }
}