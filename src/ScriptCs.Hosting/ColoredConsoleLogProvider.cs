using System;
using System.Collections.Generic;
using System.Globalization;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ColoredConsoleLogProvider : ILogProvider
    {
        private static readonly Disposable disposable = new Disposable();
        private static readonly Dictionary<LogLevel, ConsoleColor> colors =
            new Dictionary<LogLevel, ConsoleColor>
            {
                { LogLevel.Fatal, ConsoleColor.Red },
                { LogLevel.Error, ConsoleColor.DarkRed },
                { LogLevel.Warn, ConsoleColor.DarkYellow },
                { LogLevel.Info, ConsoleColor.Gray },
                { LogLevel.Debug, ConsoleColor.DarkGray },
                { LogLevel.Trace, ConsoleColor.DarkMagenta },
            };

        private readonly LogLevel _logLevel;
        private readonly IConsole _console;

        public ColoredConsoleLogProvider(LogLevel logLevel, IConsole console)
        {
            Guard.AgainstNullArgument("console", console);

            _logLevel = logLevel;
            _console = console;
        }

        public Logger GetLogger(string name)
        {
            return (logLevel, messageFunc, exception, formatParameters) =>
                Log(name, logLevel, messageFunc, exception, formatParameters);
        }

        public IDisposable OpenNestedContext(string message)
        {
            return disposable;
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return disposable;
        }

        public bool Log(
            string name, LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            Guard.AgainstNullArgument("formatParameters", formatParameters);

            var isEnabled = IsEnabled(logLevel);
            if (!isEnabled || messageFunc == null)
            {
                return isEnabled;
            }

            var prefix = logLevel == LogLevel.Info
                ? null
                : string.Concat(logLevel.ToString().ToUpperInvariant(), ": ");

            if (_logLevel == LogLevel.Debug || _logLevel == LogLevel.Trace)
            {
                prefix = string.Concat(prefix, "[", name, "] ");
            }

            var message = string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters);

            var suffix = string.Empty;
            if (exception != null)
            {
                if (_logLevel == LogLevel.Debug || _logLevel == LogLevel.Trace)
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
            if (!colors.TryGetValue(logLevel, out color))
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

            return true;
        }

        private bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Fatal:
                    return true;
                case LogLevel.Error:
                    return
                        _logLevel == LogLevel.Error ||
                        _logLevel == LogLevel.Warn ||
                        _logLevel == LogLevel.Info ||
                        _logLevel == LogLevel.Debug ||
                        _logLevel == LogLevel.Trace;
                case LogLevel.Warn:
                    return
                        _logLevel == LogLevel.Warn ||
                        _logLevel == LogLevel.Info ||
                        _logLevel == LogLevel.Debug ||
                        _logLevel == LogLevel.Trace;
                case LogLevel.Info:
                    return
                        _logLevel == LogLevel.Info ||
                        _logLevel == LogLevel.Debug ||
                        _logLevel == LogLevel.Trace;
                case LogLevel.Debug:
                    return
                        _logLevel == LogLevel.Debug ||
                        _logLevel == LogLevel.Trace;
                case LogLevel.Trace:
                    return
                        _logLevel == LogLevel.Trace;
            }

            return true;
        }

        private sealed class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
