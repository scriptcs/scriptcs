using Roslyn.Scripting;

namespace ScriptCs.Wrappers
{
    public class SessionWrapper : ISession
    {
        private Session _session;

        public SessionWrapper(Session session)
        {
            _session = session;
        }

        public IScriptEngine Engine 
        {    
            get
            {
                return new ScriptEngineWrapper(_session.Engine);
            }
        }

        public Session WrappedSession
        {
            get
            {
                return _session;
            }
        }

        public object Execute(string code)
        {
            return _session.Execute(code);
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            _session.AddReference(assemblyDisplayNameOrPath);
        }

        public void ImportNamespace(string @namespace)
        {
            _session.ImportNamespace(@namespace);
        }

        public ISubmission<T> CompileSubmission<T>(string code)
        {
            var submission = _session.CompileSubmission<T>(code);
            return new SubmissionWrapper<T>(submission);
        }
    }
}
