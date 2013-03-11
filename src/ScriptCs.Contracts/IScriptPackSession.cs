using System.Threading;

namespace ScriptCs.Contracts
{
    public interface IScriptPackSession
    {
        void AddReference(string assemblyDisplayNameOrPath);
        void ImportNamespace(string @namespace);
        void SetApartmentState(ApartmentState state);
    }
}
