using System;
using System.Collections.Generic;
using System.Globalization;
using ScriptCs.Contracts;
using ScriptCs.Logging;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    public class ScriptConsoleLogger : ILog
    {
        private readonly LogLevel _consoleLogLevel;
        private readonly IConsole _console;
        private readonly ILog _log;
        private readonly Dictionary<Logging.LogLevel, ConsoleColor> _colors =
            new Dictionary<Logging.LogLevel, ConsoleColor>
            {
                { Logging.LogLevel.Fatal, ConsoleColor.Red },
                { Logging.LogLevel.Error, ConsoleColor.DarkRed },
                { Logging.LogLevel.Warn, ConsoleColor.DarkYellow },
                { Logging.LogLevel.Info, ConsoleColor.Gray },
                { Logging.LogLevel.Debug, ConsoleColor.DarkGray },
                { Logging.LogLevel.Trace, ConsoleColor.DarkMagenta },
            };

        public ScriptConsoleLogger(LogLevel consoleLogLevel, IConsole console, ILog log)
        {
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("log", log);

            _consoleLogLevel = consoleLogLevel;
            _console = console;
            _log = log;
        }

        public bool Log(
            Logging.LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            var logged = _log.Log(logLevel, messageFunc, exception, formatParameters);
            var consoleLog = false;
            switch (logLevel)
            {
                case Logging.LogLevel.Fatal:
                    consoleLog = true;
                    break;
                case Logging.LogLevel.Error:
                    consoleLog = true;
                    break;
                case Logging.LogLevel.Warn:
                    consoleLog = true;
                    break;
                case Logging.LogLevel.Info:
                    consoleLog = _consoleLogLevel != LogLevel.Error;
                    break;
                case Logging.LogLevel.Debug:
                    consoleLog = _consoleLogLevel == LogLevel.Debug || _consoleLogLevel == LogLevel.Trace;
                    break;
                case Logging.LogLevel.Trace:
                    consoleLog = _consoleLogLevel == LogLevel.Trace;
                    break;
            }

            if (consoleLog && messageFunc != null)
            {
                var prefix = logLevel == Logging.LogLevel.Info
                    ? null
                    : string.Concat(logLevel.ToString().ToUpperInvariant(), ": ");

                var message = string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters);

                var suffix = string.Empty;
                if (exception != null)
                {
                    if (_consoleLogLevel == LogLevel.Debug || _consoleLogLevel == LogLevel.Trace)
                    {
                        var exceptions = new List<string>();
                        while (exception != null)
                        {
                            var exceptionString = string.Format(
                                CultureInfo.InvariantCulture,
                                "[{0}] {1}{2}{3}",
                                exception.GetType().FullName,
                                exception.Message,
                                Environment.NewLine,
                                exception.StackTrace);

                            exceptions.Add(exceptionString);
                            exception = exception.InnerException;
                        }

                        var divider = string.Format(
                            CultureInfo.InvariantCulture, "{0}{1}{0}", Environment.NewLine, "=== INNER EXCEPTION ===");

                        suffix = " " + string.Join(divider, exceptions);
                    }
                    else
                    {
                        suffix = string.Format(
                            CultureInfo.InvariantCulture, " [{0}] {1}", exception.GetType().Name, exception.Message);
                    }
                }

                ConsoleColor color;
                if (!_colors.TryGetValue(logLevel, out color))
                {
                    color = ConsoleColor.White;
                }

                var originalColor = _console.ForegroundColor;
                _console.ForegroundColor = color;
                try
                {
                    _console.WriteLine(string.Concat(prefix, message, suffix));
                }
                finally
                {
                    _console.ForegroundColor = originalColor;
                }
            }

            return logged || consoleLog;
        }
    }
}
