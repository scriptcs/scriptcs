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

        public RoslynScriptEngine()
        {
            _scriptEngine = new ScriptEngine();
        }

        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public void Execute(string code, IEnumerable<string> references, IScriptPackSession scriptPackSession, object hostObject = null)
        {
            var session = _scriptEngine.CreateSession(hostObject);

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