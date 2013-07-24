using Autofac;

namespace ScriptCs
{
    public interface IScriptContainerFactory
    {
        IContainer InitializationContainer { get; }
        IContainer RuntimeContainer { get; }
    }
}