using System;
using System.Globalization;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
    public class CommonLoggingLogProvider : ILogProvider
    {
        private readonly Common.Logging.ILog _logger;

        public CommonLoggingLogProvider(Common.Logging.ILog logger)
        {
            Guard.AgainstNullArgument("logger", logger);

            _logger = logger;
        }

        public Logger GetLogger(string name)
        {
            return Log;
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }

        private bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            if (messageFunc == null)
            {
                return IsEnabled(logLevel);
            }

            messageFunc = Format(messageFunc, formatParameters);

            if (exception != null)
            {
                return LogException(logLevel, messageFunc, exception);
            }

            switch (logLevel)
            {
                case LogLevel.Debug:
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug(messageFunc());
                        return true;
                    }

                    break;
                case LogLevel.Info:
                    if (_logger.IsInfoEnabled)
                    {
                        _logger.Info(messageFunc());
                        return true;
                    }

                    break;
                case LogLevel.Warn:
                    if (_logger.IsWarnEnabled)
                    {
                        _logger.Warn(messageFunc());
                        return true;
                    }

                    break;
                case LogLevel.Error:
                    if (_logger.IsErrorEnabled)
                    {
                        _logger.Error(messageFunc());
                        return true;
                    }

                    break;
                case LogLevel.Fatal:
                    if (_logger.IsFatalEnabled)
                    {
                        _logger.Fatal(messageFunc());
                        return true;
                    }
                    break;

                default:
                    if (_logger.IsTraceEnabled)
                    {
                        _logger.Trace(messageFunc());
                        return true;
                    }

                    break;
            }

            return false;
        }

        private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug(messageFunc(), exception);
                        return true;
                    }

                    break;
                case LogLevel.Info:
                    if (_logger.IsInfoEnabled)
                    {
                        _logger.Info(messageFunc(), exception);
                        return true;
                    }

                    break;
                case LogLevel.Warn:
                    if (_logger.IsWarnEnabled)
                    {
                        _logger.Warn(messageFunc(), exception);
                        return true;
                    }

                    break;
                case LogLevel.Error:
                    if (_logger.IsErrorEnabled)
                    {
                        _logger.Error(messageFunc(), exception);
                        return true;
                    }

                    break;
                case LogLevel.Fatal:
                    if (_logger.IsFatalEnabled)
                    {
                        _logger.Fatal(messageFunc(), exception);
                        return true;
                    }

                    break;
                default:
                    if (_logger.IsTraceEnabled)
                    {
                        _logger.Trace(messageFunc(), exception);
                        return true;
                    }

                    break;
            }

            return false;
        }

        private bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return _logger.IsDebugEnabled;
                case LogLevel.Info:
                    return _logger.IsInfoEnabled;
                case LogLevel.Warn:
                    return _logger.IsWarnEnabled;
                case LogLevel.Error:
                    return _logger.IsErrorEnabled;
                case LogLevel.Fatal:
                    return _logger.IsFatalEnabled;
                default:
                    return _logger.IsTraceEnabled;
            }
        }

        private static Func<string> Format(Func<string> messageFunc, object[] formatParameters)
        {
            if (formatParameters == null || formatParameters.Length == 0)
            {
                return messageFunc;
            }

            return () =>
            {
                var format = messageFunc();
                try
                {
                    return string.Format(CultureInfo.InvariantCulture, format, formatParameters);
                }
                catch (FormatException ex)
                {
                    throw new FormatException("The input string '" + format + "' could not be formatted using string.Format", ex);
                }
            };
        }
    }
}
