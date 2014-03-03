using System.Text;

using log4net.Appender;
using log4net.Core;

using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ScriptConsoleAppender : AppenderSkeleton
    {
        private readonly IConsole _console;

        public ScriptConsoleAppender(IConsole console)
        {
            _console = console;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            Guard.AgainstNullArgument("loggingEvent", loggingEvent);

            var txt = new StringBuilder();
            txt.Append(loggingEvent.Level);
            txt.Append(": ");
            txt.Append(loggingEvent.RenderedMessage);
            _console.WriteLine(txt.ToString());
        }
    }
}