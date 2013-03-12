using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Wrappers
{
    internal class ScriptEngineWrapper : IScriptEngine
    {
        private CommonScriptEngine _scriptEngine;

        public ScriptEngineWrapper()
        {
            _scriptEngine = new ScriptEngine();
        }

        public ScriptEngineWrapper(CommonScriptEngine engine)
        {
            _scriptEngine = engine;
        }

        public string BaseDirectory
        {
            get
            {
                return _scriptEngine.BaseDirectory;
            }

            set
            {
                _scriptEngine.BaseDirectory = value;
            }
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            _scriptEngine.AddReference(assemblyDisplayNameOrPath);
        }

        public ISession CreateSession()
        {
            return new SessionWrapper(_scriptEngine.CreateSession());
        }

        public ISession CreateSession<THostObject>(THostObject hostObject) where THostObject : class
        {
            return new SessionWrapper(_scriptEngine.CreateSession(hostObject));
        }

        public ISession CreateSession(object hostObject, System.Type hostObjectType = null)
        {
            return new SessionWrapper(_scriptEngine.CreateSession(hostObject, hostObjectType));
        }
    }
}