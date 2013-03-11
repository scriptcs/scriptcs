using System.Threading;

using Roslyn.Scripting;

namespace ScriptCs
{
    public interface ISession
    {
        IScriptEngine Engine { get; }
        Session WrappedSession { get; }
        object Execute(string code);
        void AddReference(string assemblyDisplayNameOrPath);
        void ImportNamespace(string @namespace);
        ISubmission<T> CompileSubmission<T>(string code);
        void SetApartmentState(ApartmentState apartmentState);
    }
}
