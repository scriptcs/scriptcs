using System.Text;
using ScriptCs.Contracts;
using log4net.Appender;
using log4net.Core;

namespace ScriptCs
{
    public class ScriptcsConsoleAppender : AppenderSkeleton
    {
        private readonly IConsole _console;

        public ScriptcsConsoleAppender(IConsole console)
        {
            _console = console;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var txt = new StringBuilder();
            txt.Append(loggingEvent.Level);
            txt.Append(": ");
            txt.Append(loggingEvent.RenderedMessage);
            _console.WriteLine(txt.ToString());
        }
    }
}