using Common.Logging;
using Common.Logging.Factory;
using ScriptCs.Contracts;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    public class ScriptConsoleLogger : AbstractLogger
    {
        private readonly LogLevel _consoleLogLevel;
        private readonly IConsole _console;
        private readonly ILog _log;

        public ScriptConsoleLogger(LogLevel consoleLogLevel, IConsole console, ILog log)
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

        protected override void WriteInternal(Common.Logging.LogLevel level, object message, System.Exception exception)
        {
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
                _console.WriteLine(string.Concat(level.ToString().ToUpperInvariant(), ": ", message.ToString()));
            }
        }
    }
}
