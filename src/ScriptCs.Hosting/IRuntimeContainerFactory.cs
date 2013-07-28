using Autofac;

namespace ScriptCs
{
    public interface IRuntimeContainerFactory
    {
        ScriptServices GetScriptServices();
    }
}