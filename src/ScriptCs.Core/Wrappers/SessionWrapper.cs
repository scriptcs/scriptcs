using Roslyn.Scripting;
using ScriptCs.Contracts;

namespace ScriptCs.Wrappers
{
    public class SessionWrapper : ISession
    {
        private Session _session;

        public SessionWrapper(Session session)
        {
            this._session = session;
        }

        public object Execute(string code)
        {
            return this._session.Execute(code);
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            this._session.AddReference(assemblyDisplayNameOrPath);
        }
    }
}
