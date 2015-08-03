namespace ScriptCs.Contracts
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class LogProviderExtensions
    {
        public static ILog For<T>(this ILogProvider provider)
        {
            return provider.For(typeof(T));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILog ForCurrentType(this ILogProvider provider)
        {
            return provider.For(new StackFrame(1, false).GetMethod().DeclaringType);
        }

        public static ILog For(this ILogProvider provider, Type type)
        {
            return provider.For(type.FullName);
        }

        public static ILog For(this ILogProvider provider, string name)
        {
            return new LoggerExecutionWrapper(provider.GetLogger(name));
        }

        private class LoggerExecutionWrapper : ILog
        {
            private const string FailedToGenerateLogMessage = "Failed to generate log message";

            private readonly Logger _logger;

            internal LoggerExecutionWrapper(Logger logger)
            {
                _logger = logger;
            }

            public bool Log(
                LogLevel logLevel, Func<string> createMessage, Exception exception = null, params object[] formatArgs)
            {
                if (createMessage == null)
                {
                    return _logger(logLevel, null);
                }

                Func<string> wrappedMessageFunc = () =>
                {
                    try
                    {
                        return createMessage();
                    }
                    catch (Exception ex)
                    {
                        Log(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
                    }
                    
                    return null;
                };

                return _logger(logLevel, wrappedMessageFunc, exception, formatArgs);
            }
        }
    }
}
