using Roslyn.Scripting;

namespace ScriptCs.Wrappers
{
    public class SessionWrapper : ISession
    {
        private Session session;

        public SessionWrapper(Session session)
        {
            this.session = session;
        }

        public object Execute(string code)
        {
            return this.session.Execute(code);
        }
    }
}
