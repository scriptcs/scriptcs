using System;
using ScriptCs.Contracts.Logging;

namespace ScriptCs.Tests
{
    using System.Globalization;
    using System.Text;

    public class TestLogger : ILog
    {
        private readonly StringBuilder _output = new StringBuilder();

        public bool Log(
            LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            if (messageFunc != null)
            {
                var message = string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters);
                var text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", logLevel.ToString().ToUpper(), message);
                this._output.AppendLine(text);
            }

            return true;
        }

        public string Output
        {
            get { return this._output.ToString(); }
        }
    }
}
