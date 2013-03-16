using System.Collections.Generic;
using System.Linq;
using log4net;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptEngine : IScriptEngine
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;
        private readonly ILog _logger;

        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptEngine = new ScriptEngine();

            _scriptHostFactory = scriptHostFactory;
            _logger = logger;
        }

        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public void Execute(string code, IEnumerable<string> references, ScriptPackSession scriptPackSession)
        {
            _logger.Debug("Retrieving script packs contexts");
            var contexts = scriptPackSession.ScriptPacks.Select(x => x.GetContext());
            
            _logger.Debug("Creating script host");
            var host = _scriptHostFactory.CreateScriptHost(contexts);

            _logger.Debug("Creating session");
            var session = _scriptEngine.CreateSession(host);

            foreach (var reference in references.Union(scriptPackSession.References).Distinct())
            {
                _logger.InfoFormat("Adding reference to {0}", reference);
                session.AddReference(reference);
            }
            
            foreach (var @namespace in scriptPackSession.Namespaces.Distinct())
            {
                _logger.InfoFormat("Importing namespace {0}", @namespace);
                session.ImportNamespace(@namespace);    
            }

            _logger.Debug("Executing code");
            Execute(code, session);
        }

        protected virtual void Execute(string code, Session session)
        {
            session.Execute(code);
        }
    }
}