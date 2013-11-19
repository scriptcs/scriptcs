namespace ScriptCs.Contracts
{
    public interface IModuleConfiguration : IServiceOverrides<IModuleConfiguration>
    {
        bool Cache { get; }

        string ScriptName { get; }

        bool Repl { get; }

        LogLevel LogLevel { get; }
    }
}