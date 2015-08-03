namespace ScriptCs.Contracts
{
    using System;

    public class DefaultLogProvider : ILogProvider
    {
        private readonly ILogProvider _provider = ResolveLogProvider() ?? new NullLogProvider();

        [Obsolete("Should not be called directly. Instead, call a method on LogProviderExtensions.")]
        public Logger GetLogger(string name)
        {
            return _provider.GetLogger(name);
        }

        /// <summary>
        /// Opens a nested diagnostics context. Not supported in <seealso cref="http://msdn.microsoft.com/entlib"/>.
        /// </summary>
        /// <param name="message">The message to add to the diagnostics context.</param>
        /// <returns>A disposable that when disposed removes the message from the context.</returns>
        public IDisposable OpenNestedContext(string message)
        {
            return _provider.OpenNestedContext(message);
        }

        /// <summary>
        /// Opens a mapped diagnostics context. Not supported in <seealso cref="http://msdn.microsoft.com/entlib"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>A disposable that when disposed removes the map from the context.</returns>
        public IDisposable OpenMappedContext(string key, string value)
        {
            return _provider.OpenMappedContext(key, value);
        }

        private static ILogProvider ResolveLogProvider()
        {
            var libLogProvider = LibLog.LogProvider.ResolveLogProvider();
            return libLogProvider == null ? null : new LibLogAdapter(libLogProvider);
        }

        private class NullLogProvider : ILogProvider
        {
            private static readonly Logger logger = (_, __, ___, ____) => false;
            private static readonly Disposable disposable = new Disposable();

            [Obsolete("Should not be called directly. Instead, call a method on LogProviderExtensions.")]
            public Logger GetLogger(string name)
            {
                return logger;
            }

            public IDisposable OpenNestedContext(string message)
            {
                return disposable;
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                return disposable;
            }

            private sealed class Disposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }

        private class LibLogAdapter : ILogProvider
        {
            private readonly LibLog.ILogProvider _libLogProvider;

            public LibLogAdapter(LibLog.ILogProvider libLogProvider)
            {
                _libLogProvider = libLogProvider;
            }

            public Logger GetLogger(string name)
            {
                return (logLevel, messageFunc, exception, formatParameters) =>
                    _libLogProvider.GetLogger(name)((LibLog.LogLevel)logLevel, messageFunc, exception, formatParameters);
            }

            public IDisposable OpenNestedContext(string message)
            {
                return _libLogProvider.OpenNestedContext(message);
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                return _libLogProvider.OpenMappedContext(key, value);
            }
        }
    }
}
