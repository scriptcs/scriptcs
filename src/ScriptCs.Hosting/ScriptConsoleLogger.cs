using System;
using System.Collections.Generic;
using ScriptCs.Contracts;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
    public class ScriptConsoleLogger : Common.Logging.Factory.AbstractLogger
    {
        private readonly LogLevel _consoleLogLevel;
        private readonly IConsole _console;
        private readonly Common.Logging.ILog _log;
        private readonly Dictionary<Common.Logging.LogLevel, ConsoleColor> colors =
            new Dictionary<Common.Logging.LogLevel, ConsoleColor>
            {
                { Common.Logging.LogLevel.Fatal, ConsoleColor.Red },
                { Common.Logging.LogLevel.Error, ConsoleColor.DarkRed },
                { Common.Logging.LogLevel.Warn, ConsoleColor.DarkYellow },
                { Common.Logging.LogLevel.Info, ConsoleColor.Gray },
                { Common.Logging.LogLevel.Debug, ConsoleColor.DarkGray },
                { Common.Logging.LogLevel.Trace, ConsoleColor.DarkMagenta },
            };

        public ScriptConsoleLogger(LogLevel consoleLogLevel, IConsole console, Common.Logging.ILog log)
        {
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("log", log);

            _consoleLogLevel = consoleLogLevel;
            _console = console;
            _log = log;
        }

        public override bool IsFatalEnabled
        {
            get { return true; }
        }

        public override bool IsErrorEnabled
        {
            get { return true; }
        }

        public override bool IsWarnEnabled
        {
            get { return true; }
        }

        public override bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled || _consoleLogLevel != LogLevel.Error; }
        }

        public override bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled || _consoleLogLevel == LogLevel.Debug || _consoleLogLevel == LogLevel.Trace; }
        }

        public override bool IsTraceEnabled
        {
            get { return _log.IsTraceEnabled || _consoleLogLevel == LogLevel.Trace; }
        }

        protected override void WriteInternal(Common.Logging.LogLevel level, object message, Exception exception)
        {
            Guard.AgainstNullArgument("message", message);

            var consoleLog = false;
            switch (level)
            {
                case Common.Logging.LogLevel.Fatal:
                    consoleLog = true;
                    if (_log.IsFatalEnabled)
                    {
                        _log.Fatal(message, exception);
                    }

                    break;
                case Common.Logging.LogLevel.Error:
                    consoleLog = true;
                    if (_log.IsErrorEnabled)
                    {
                        _log.Error(message, exception);
                    }

                    break;
                case Common.Logging.LogLevel.Warn:
                    consoleLog = true;
                    if (_log.IsWarnEnabled)
                    {
                        _log.Warn(message, exception);
                    }

                    break;
                case Common.Logging.LogLevel.Info:
                    consoleLog = _consoleLogLevel != LogLevel.Error;
                    if (_log.IsInfoEnabled)
                    {
                        _log.Info(message, exception);
                    }

                    break;
                case Common.Logging.LogLevel.Debug:
                    consoleLog = _consoleLogLevel == LogLevel.Debug || _consoleLogLevel == LogLevel.Trace;
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug(message, exception);
                    }

                    break;
                case Common.Logging.LogLevel.Trace:
                    consoleLog = _consoleLogLevel == LogLevel.Trace;
                    if (_log.IsTraceEnabled)
                    {
                        _log.Trace(message, exception);
                    }

                    break;
            }

            if (consoleLog)
            {
                var prefix = level == Common.Logging.LogLevel.Info
                    ? null
                    : string.Concat(level.ToString().ToUpperInvariant(), ": ");

                ConsoleColor color;
                if (!colors.TryGetValue(level, out color))
                {
                    color = ConsoleColor.White;
                }

                var originalColor = _console.ForegroundColor;
                _console.ForegroundColor = color;
                try
                {
                    _console.WriteLine(string.Concat(prefix, message.ToString()));
                }
                finally
                {
                    _console.ForegroundColor = originalColor;
                }
            }
        }
    }
}
