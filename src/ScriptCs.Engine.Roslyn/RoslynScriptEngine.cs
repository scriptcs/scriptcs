using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptEngine : IScriptEngine
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;
        private readonly ILog _logger;
        public const string SessionKey = "Session";

        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptEngine = new ScriptEngine();
            _scriptEngine.AddReference(typeof(ScriptExecutor).Assembly);
            _scriptHostFactory = scriptHostFactory;
            _logger = logger;
        }
        
        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public void Execute(string code, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            _logger.Info("Starting to create execution components");
            _logger.Debug("Creating script host");
            Session session;
            
            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts));
                _logger.Debug("Creating session");
                session = _scriptEngine.CreateSession(host);
                scriptPackSession.State[SessionKey] = session;
 
                foreach (var reference in references.Union(scriptPackSession.References).Distinct())
                {
                    _logger.DebugFormat("Adding reference to {0}", reference);
                    session.AddReference(reference);
                }

                foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                {
                    _logger.DebugFormat("Importing namespace {0}", @namespace);
                    session.ImportNamespace(@namespace);
                }
 
            }
            else
            {
                _logger.Debug("Reusing existing session");
                session = (Session) scriptPackSession.State[SessionKey];
            }

            _logger.Info("Starting execution");
            Execute(code, session);
            _logger.Info("Finished execution");
        }

        protected virtual void Execute(string code, Session session)
        {
            session.Execute(code);
        }
    }
}