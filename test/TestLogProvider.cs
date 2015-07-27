using System;
using System.Globalization;
using System.Text;

namespace ScriptCs.Tests
{
    using ScriptCs.Contracts;

    public class TestLogProvider : ILogProvider
    {
        private static readonly Disposable disposable = new Disposable();
        private readonly StringBuilder _output = new StringBuilder();

        public string Output
        {
            get { return _output.ToString(); }
        }

        public Logger GetLogger(string name)
        {
            return Log;
        }

        public IDisposable OpenNestedContext(string message)
        {
            return disposable;
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return disposable;
        }

        private bool Log(
            LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            if (messageFunc != null)
            {
                var line = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1}",
                    logLevel.ToString().ToUpper(),
                    string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters));

                _output.AppendLine(line);
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
