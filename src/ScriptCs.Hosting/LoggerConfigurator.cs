using System;
using System.Globalization;
using Common.Logging.Log4Net;
using log4net;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using ScriptCs.Contracts;
using ICommonLog = Common.Logging.ILog;

namespace ScriptCs
{
    public class LoggerConfigurator : ILoggerConfigurator
    {
        private const string ThreadPattern = " Thread[%thread]";
        private const string Pattern = "%-5level{threadLevel}: %message%newline";
        private const string LoggerName = "scriptcs";

        private readonly LogLevel _logLevel;

        private ICommonLog _logger;

        public LoggerConfigurator(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Configure(IConsole console)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var logger = LogManager.GetLogger(LoggerName);
            var consoleAppender = new ScriptConsoleAppender(console)
            {
                Layout = new PatternLayout(GetLogPattern(_logLevel)),
                Threshold = hierarchy.LevelMap[_logLevel.ToString().ToUpper(CultureInfo.CurrentCulture)]
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

        private static string GetLogPattern(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Error:
                case LogLevel.Info:
                    return Pattern.Replace("{threadLevel}", string.Empty);
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return Pattern.Replace("{threadLevel}", ThreadPattern);
                default:
                    throw new ArgumentOutOfRangeException("logLevel");
            }
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