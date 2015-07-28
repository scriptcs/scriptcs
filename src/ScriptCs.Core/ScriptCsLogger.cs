using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
    public class ScriptCsLogger : Common.Logging.Factory.AbstractLogger
    {
        private readonly ILog _log;

        public ScriptCsLogger(ILog log)
        {
            Guard.AgainstNullArgument("log", log);

            _log = log;
        }

        protected override void WriteInternal(Common.Logging.LogLevel level, object message, Exception exception)
        {
            if (level == Common.Logging.LogLevel.Off)
            {
                return;
            }

            _log.Log(Map(level), () => message == null ? null : message.ToString(), exception);
        }

        public override bool IsTraceEnabled
        {
            get { return _log.IsTraceEnabled(); }
        }

        public override bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled(); }
        }

        public override bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled(); }
        }

        public override bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled(); }
        }

        public override bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled(); }
        }

        public override bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled(); }
        }

        private static LogLevel Map(Common.Logging.LogLevel level)
        {
            switch (level)
            {
                case Common.Logging.LogLevel.All:
                    return LogLevel.Trace;
                case Common.Logging.LogLevel.Debug:
                    return LogLevel.Debug;
                case Common.Logging.LogLevel.Error:
                    return LogLevel.Error;
                case Common.Logging.LogLevel.Fatal:
                    return LogLevel.Fatal;
                case Common.Logging.LogLevel.Info:
                    return LogLevel.Info;
                case Common.Logging.LogLevel.Trace:
                    return LogLevel.Trace;
                case Common.Logging.LogLevel.Warn:
                    return LogLevel.Warn;
            }

            throw new NotSupportedException("Unknown log level.");
        }
    }
}
