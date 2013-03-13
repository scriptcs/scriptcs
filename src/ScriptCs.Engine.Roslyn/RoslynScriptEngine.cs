using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    [Export(Constants.RunContractName, typeof(IScriptEngine))]
    public class RoslynScriptEngine : IScriptEngine
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;

        [ImportingConstructor]
        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory)
        {
            _scriptEngine = new ScriptEngine();

            _scriptHostFactory = scriptHostFactory;
        }

        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public void Execute(string code, IEnumerable<string> references, ScriptPackSession scriptPackSession)
        {
            var contexts = scriptPackSession.ScriptPacks.Select(x => x.GetContext());
            var host = _scriptHostFactory.CreateScriptHost(contexts);

            var session = _scriptEngine.CreateSession(host);

            foreach (var reference in references)
                session.AddReference(reference);

            foreach (var reference in scriptPackSession.References)
                session.AddReference(reference);

            Execute(code, session);
        }

        protected virtual void Execute(string code, Session session)
        {
            session.Execute(code);
        }
    }
}