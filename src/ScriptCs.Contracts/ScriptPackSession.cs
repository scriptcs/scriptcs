using System.Collections.Generic;
using System.Linq;


namespace ScriptCs.Contracts
{
    public class ScriptPackSession : IScriptPackSession
    {
        private readonly IList<IScriptPack> _scriptPacks;

        private readonly string[] _scriptArgs;

        private readonly IList<IScriptPackContext> _contexts;

        private readonly IDictionary<string, object> _state;

        private readonly IList<string> _references;

        private readonly IList<string> _namespaces;

        public ScriptPackSession(IEnumerable<IScriptPack> scriptPacks, string[] scriptArgs)
        {
            _scriptPacks = scriptPacks.ToList();
            _scriptArgs = scriptArgs;
            _contexts = _scriptPacks.Select(s => s.GetContext()).Where(c => c != null).ToList();
            _references = new List<string>();
            _namespaces = new List<string>();
            _state = new Dictionary<string, object>();
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

        public IDictionary<string, object> State
        {
            get { return _state; }
        }

        public string[] ScriptArgs
        {
            get { return _scriptArgs; }
        }

        void IScriptPackSession.AddReference(string assemblyDisplayNameOrPath)
        {
            _references.Add(assemblyDisplayNameOrPath);
        }

        void IScriptPackSession.ImportNamespace(string @namespace)
        {
            _namespaces.Add(@namespace);
        }

        public void AddScriptPack(IScriptPack pack)
        {
            pack.Initialize(this);
            _scriptPacks.Add(pack);
            var context = pack.GetContext();
            _contexts.Add(context);
        }
    }
}
