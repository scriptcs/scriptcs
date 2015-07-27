namespace ScriptCs.Contracts
{
    using System;

    public interface ILog
    {
        /// <summary>
        /// Logs a message for a specified log level, if the <see cref="ILog"/> is enabled for that log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="createMessage">
        /// Optional function which creates the message.
        /// Specify <c>null</c> to simply check if the logger is enabled for the <paramref name="logLevel"/>.
        /// </param>
        /// <param name="exception">An optional exception to include with the message.</param>
        /// <param name="formatArgs">
        /// Optional arguments for formatting the message created by <paramref name="createMessage"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="ILog"/> is enabled for the <paramref name="logLevel"/>.
        /// Otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Note to implementers for optimal performance:
        /// When the <paramref name="createMessage"/> is <c>null</c>, simply return a <see cref="bool"/>
        /// indicating whether the <see cref="ILog"/> is enabled for the specified <paramref name="logLevel"/>.
        /// When the <paramref name="createMessage"/> is not <c>null</c>, it should only be invoked
        /// if the <see cref="ILog"/> is enabled for the specified <paramref name="logLevel"/>.
        /// </remarks>
        bool Log(
            LogLevel logLevel,
            Func<string> createMessage = null,
            Exception exception = null,
            params object[] formatArgs);
    }
}
