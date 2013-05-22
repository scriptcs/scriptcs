﻿using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackSession : IScriptPackSession
    {
        private readonly IEnumerable<IScriptPack> _scriptPacks;
        private readonly IEnumerable<IScriptPackContext> _contexts;
        private readonly IDictionary<string, object> _state;

        private IList<string> _references;
        private IList<string> _namespaces;
        private IList<string> _scripts;

        public ScriptPackSession(IEnumerable<IScriptPack> scriptPacks)
        {
            _scriptPacks = scriptPacks;
            _contexts = _scriptPacks.Select(s => s.GetContext()).Where(c=>c != null);
            _references = new List<string>();
            _namespaces = new List<string>();
            _scripts = new List<string>();
            _state = new Dictionary<string, object>();
            AddScriptContextNamespace();
        }

        public void AddInitializationScripts(IEnumerable<string> scripts)
        {
            foreach(var script in scripts)
                _scripts.Add(script);
        }

        private void AddScriptContextNamespace()
        {
            foreach (var context in _contexts)
            {
                _namespaces.Add(context.GetType().Namespace);
            }
        }

        public IEnumerable<string> Scripts
        {
            get { return _scripts; }
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

        public IDictionary<string, object> State
        {
            get { return _state; }
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