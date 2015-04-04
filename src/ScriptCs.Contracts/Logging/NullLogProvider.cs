using System;

namespace ScriptCs.Contracts.Logging
{
    public class NullLogProvider : ILogProvider
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
}
