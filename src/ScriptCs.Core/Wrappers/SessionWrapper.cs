using System.Threading;

using Roslyn.Scripting;

namespace ScriptCs.Wrappers
{
    public class SessionWrapper : ISession
    {
        private Session _session;

        private ApartmentState _apartmentState;

        public SessionWrapper(Session session)
        {
            _session = session;
            _apartmentState = ApartmentState.MTA;
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
            object result = null;

            var thread = new Thread(() => result = _session.Execute(code));
            thread.SetApartmentState(_apartmentState);
            thread.Start();
            thread.Join();

            return result;
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

        public void SetApartmentState(ApartmentState apartmentState)
        {
            _apartmentState = apartmentState;
        }
    }
}
