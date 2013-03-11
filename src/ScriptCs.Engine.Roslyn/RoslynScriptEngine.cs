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

        public IScriptHostFactory ScriptHostFactory { get; set; }

        public void Execute(string code, IEnumerable<string> references, IEnumerable<IScriptPack> scriptPacks)
        {
            var contexts = scriptPacks.Select(x => x.GetContext());
            var host = ScriptHostFactory.CreateScriptHost(contexts);

            var session = _scriptEngine.CreateSession(host);

            foreach (var reference in references)
                session.AddReference(reference);

            var scriptPackSession = new RoslynScriptPackSession(session);

            foreach (var pack in scriptPacks)
                pack.Initialize(scriptPackSession);

            Execute(code, session);

            foreach (var pack in scriptPacks)
                pack.Terminate();
        }

        protected virtual void Execute(string code, Session session)
        {
            session.Execute(code);
        }
    }
}