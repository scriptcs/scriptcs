using Autofac;

namespace ScriptCs
{
    public interface IRuntimeContainerFactory
    {
        IContainer Container { get; }
    }
}