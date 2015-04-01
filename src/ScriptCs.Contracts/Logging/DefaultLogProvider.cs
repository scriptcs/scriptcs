using System;

namespace ScriptCs.Contracts.Logging
{
    public class DefaultLogProvider : ILogProvider
    {
        private readonly ILogProvider _provider = LogProvider.ResolveLogProvider() ?? new NullLogProvider();

        public Logger GetLogger(string name)
        {
            return _provider.GetLogger(name);
        }

        public IDisposable OpenNestedContext(string message)
        {
            return _provider.OpenNestedContext(message);
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return _provider.OpenMappedContext(key, value);
        }
    }
}
