namespace ScriptCs.Contracts
{
    public interface IModuleMetadata
    {
        string Name { get; }

        string Extensions { get; }

        bool Autoload { get; }
    }
}