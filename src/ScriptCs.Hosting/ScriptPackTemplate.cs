using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackTemplate : IScriptPack
    {
        private IScriptPackSettings _settings;

        public ScriptPackTemplate(IScriptPackSettings settings)
        {
            _settings = settings;
        }

        void IScriptPack.Initialize(IScriptPackSession session)
        {
            (_settings.GetImports().ToList()).ForEach(session.ImportNamespace);
            ((_settings.GetReferences().ToList())).ForEach(session.AddReference);
        }

        void IScriptPack.Terminate()
        {
        }

        public Func<Type, IScriptPackContext> ContextResolver { get; set; }

        IScriptPackContext IScriptPack.GetContext()
        {
            return ContextResolver(_settings.GetContextType());
        }
    }
}
