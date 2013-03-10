namespace ScriptCs
{
    public interface ISession
    {
        IScriptEngine Engine { get; }
        object Execute(string code);
        void AddReference(string assemblyDisplayNameOrPath);
        void ImportNamespace(string @namespace);
        ISubmission<T> CompileSubmission<T>(string code);
    }
}
