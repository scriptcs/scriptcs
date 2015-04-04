namespace ScriptCs.Contracts
{
    using System;

    public interface ILogProvider
    {
        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference.</returns>
        /// <remarks>
        /// Do not call this method directly. Instead, call a method on <see cref="LogProviderExtensions"/>.
        /// </remarks>
        Logger GetLogger(string name);

        IDisposable OpenNestedContext(string message);

        IDisposable OpenMappedContext(string key, string value);
    }
}
