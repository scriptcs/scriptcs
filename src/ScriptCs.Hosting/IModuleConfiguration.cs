namespace ScriptCs
{
    public interface IModuleConfiguration : IScriptServiceConfiguration<IModuleConfiguration>
    {
        bool Debug { get; }
        string ScriptName { get; }
        bool Repl { get; }
        LogLevel LogLevel { get; }
    }
}