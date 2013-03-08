namespace ScriptCs
{
    public interface ISession
    {
        object Execute(string code);
        void AddReference(string assemblyDisplayNameOrPath);
        void ImportNamespace(string @namespace);
    }
}
