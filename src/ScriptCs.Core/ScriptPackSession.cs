using System;
using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackSession : IScriptPackSession, IDisposable
    {
        private readonly IEnumerable<IScriptPack> _scriptPacks;

        private readonly IList<string> _references;
        private readonly IList<string> _namespaces;

        public ScriptPackSession(IEnumerable<IScriptPack> scriptPacks)
        {
            _scriptPacks = scriptPacks;

            _references = new List<string>();
            _namespaces = new List<string>();

            InitializePacks();
        }

        public IEnumerable<IScriptPack> ScriptPacks
        {
            get { return _scriptPacks; }
        }

        public IEnumerable<string> References
        {
            get { return _references; }
        }

        public IEnumerable<string> Namespaces
        {
            get { return _namespaces; }
        }

        void IScriptPackSession.AddReference(string assemblyDisplayNameOrPath)
        {
            _references.Add(assemblyDisplayNameOrPath);
        }

        void IScriptPackSession.ImportNamespace(string @namespace)
        {
            _namespaces.Add(@namespace);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                TerminatePacks();
            }
        }

        private void InitializePacks()
        {
            foreach (var s in _scriptPacks)
            {
                s.Initialize(this);
            }
        }

        private void TerminatePacks()
        {
            foreach (var s in _scriptPacks)
            {
                s.Terminate();
            }
        }
    }
}
