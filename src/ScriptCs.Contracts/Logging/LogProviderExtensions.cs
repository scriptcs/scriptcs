namespace ScriptCs.Contracts.Logging
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
    }
}
