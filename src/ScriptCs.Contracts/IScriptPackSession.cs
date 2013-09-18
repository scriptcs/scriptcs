namespace ScriptCs.Contracts
{
    public interface IScriptPackSession
    {
        string[] ScriptArgs { get; }

        void AddReference(string assemblyDisplayNameOrPath);

        void ImportNamespace(string @namespace);
    }
}
