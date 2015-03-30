using System;
using System.Collections.Generic;
using System.Globalization;
using ScriptCs.Contracts;
using ScriptCs.Contracts.Logging;

namespace ScriptCs.Hosting
{
    public class ScriptConsoleLogger : ILog
    {
        private readonly LogLevel _consoleLogLevel;
        private readonly IConsole _console;
        private readonly Dictionary<LogLevel, ConsoleColor> _colors =
            new Dictionary<LogLevel, ConsoleColor>
            {
                { LogLevel.Fatal, ConsoleColor.Red },
                { LogLevel.Error, ConsoleColor.DarkRed },
                { LogLevel.Warn, ConsoleColor.DarkYellow },
                { LogLevel.Info, ConsoleColor.Gray },
                { LogLevel.Debug, ConsoleColor.DarkGray },
                { LogLevel.Trace, ConsoleColor.DarkMagenta },
            };

        public ScriptConsoleLogger(LogLevel consoleLogLevel, IConsole console)
        {
            Guard.AgainstNullArgument("console", console);

            _consoleLogLevel = consoleLogLevel;
            _console = console;
        }

        public bool Log(
            LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            var consoleLog = false;
            switch (logLevel)
            {
                case LogLevel.Fatal:
                    consoleLog = true;
                    break;
                case LogLevel.Error:
                    consoleLog =
                        _consoleLogLevel == LogLevel.Error ||
                        _consoleLogLevel == LogLevel.Warn ||
                        _consoleLogLevel == LogLevel.Info ||
                        _consoleLogLevel == LogLevel.Debug ||
                        _consoleLogLevel == LogLevel.Trace;
                    break;
                case LogLevel.Warn:
                    consoleLog =
                        _consoleLogLevel == LogLevel.Warn ||
                        _consoleLogLevel == LogLevel.Info ||
                        _consoleLogLevel == LogLevel.Debug ||
                        _consoleLogLevel == LogLevel.Trace;
                    break;
                case LogLevel.Info:
                    consoleLog =
                        _consoleLogLevel == LogLevel.Info ||
                        _consoleLogLevel == LogLevel.Debug ||
                        _consoleLogLevel == LogLevel.Trace;
                    break;
                case LogLevel.Debug:
                    consoleLog =
                        _consoleLogLevel == LogLevel.Debug ||
                        _consoleLogLevel == LogLevel.Trace;
                    break;
                case LogLevel.Trace:
                    consoleLog =
                        _consoleLogLevel == LogLevel.Trace;
                    break;
            }

            if (consoleLog && messageFunc != null)
            {
                var prefix = logLevel == LogLevel.Info
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

            return consoleLog;
        }
    }
}
