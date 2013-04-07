using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackSession : IScriptPackSession
    {
        private readonly IEnumerable<IScriptPack> _scriptPacks;
        private readonly IEnumerable<IScriptPackContext> _contexts; 

        private IList<string> _references;
        private IList<string> _namespaces;
 
        public ScriptPackSession(IEnumerable<IScriptPack> scriptPacks)
        {
            _scriptPacks = scriptPacks;
            _contexts = _scriptPacks.Select(x => x.GetContext());
            _references = new List<string>();
            _namespaces = new List<string>();

            AddScriptContextNamespace();
        }

        private void AddScriptContextNamespace()
        {
            foreach (var context in _contexts)
            {
                _namespaces.Add(context.GetType().Namespace);
            }
        }

        public IEnumerable<IScriptPackContext> Contexts
        {
            get { return _contexts; }
        } 

        public IEnumerable<string> References
        {
            get { return _references; }
        }

        public IEnumerable<string> Namespaces
        {
            get { return _namespaces; }
        }

        public void InitializePacks()
        {
            foreach (var s in _scriptPacks)
            {
                s.Initialize(this);
            }
        }

        public void TerminatePacks()
        {
            foreach (var s in _scriptPacks)
            {
                s.Terminate();
            }
        }

        void IScriptPackSession.AddReference(string assemblyDisplayNameOrPath)
        {
            _references.Add(assemblyDisplayNameOrPath);
        }

        void IScriptPackSession.ImportNamespace(string @namespace)
        {
            _namespaces.Add(@namespace);
        }
    }
}