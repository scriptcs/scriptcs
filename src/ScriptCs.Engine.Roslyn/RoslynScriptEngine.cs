﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptEngine : IScriptEngine
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;

        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory)
        {
            _scriptEngine = new ScriptEngine();
            _scriptEngine.AddReference(typeof(ScriptExecutor).Assembly);
            _scriptHostFactory = scriptHostFactory;
        }

        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public void Execute(string code, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts));
  
            var session = _scriptEngine.CreateSession(host);

            foreach (var reference in references.Union(scriptPackSession.References).Distinct())
                session.AddReference(reference);

            foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                session.ImportNamespace(@namespace);

            Execute(code, session);
        }

        protected virtual void Execute(string code, Session session)
        {
            session.Execute(code);
        }
    }
}