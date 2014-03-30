namespace ScriptCs.Contracts
{
    public interface IModule
    {
        void Initialize(IServiceOverrides<IModuleConfiguration> config);
    }
}
