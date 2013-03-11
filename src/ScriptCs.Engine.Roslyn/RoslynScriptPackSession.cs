using System;
using System.Linq;
using Roslyn.Scripting;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptPackSession : IScriptPackSession
    {
        private readonly Session _session;

        internal RoslynScriptPackSession(Session session)
        {
            _session = session;
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            _session.AddReference(assemblyDisplayNameOrPath);
        }

        public void ImportNamespace(string ns)
        {
            _session.ImportNamespace(ns);
        }
    }
}
