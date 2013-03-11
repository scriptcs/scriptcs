using System.Threading;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackSession : IScriptPackSession
    {
        private readonly ISession _session;

        public ScriptPackSession(ISession session)
        {
            _session = session;
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            _session.AddReference(assemblyDisplayNameOrPath);
        }

        public void ImportNamespace(string ns)
        {
            
        }

        public void SetApartmentState(ApartmentState state)
        {
            _session.SetApartmentState(state);
        }
    }
}
