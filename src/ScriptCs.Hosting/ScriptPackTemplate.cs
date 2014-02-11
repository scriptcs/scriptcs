using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public abstract class ScriptPackTemplate : IScriptPack
    {
        private IScriptPackSettings _settings;

        public ScriptPackTemplate(IScriptPackSettings settings) : this()
        {
            _settings = settings;
        }

        public ScriptPackTemplate()
        {
            var init = this.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (init == null)
            {
                throw new InvalidOperationException("Cannot load script pack as 'Init' method is missing");
            }
            init.Invoke(this, null);
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

        public IScriptPackSettingsReferences ScriptPack<TScriptPackContext>() where TScriptPackContext : IScriptPackContext
        {
            if (_settings == null)
            {
                _settings = new ScriptPackSettings(typeof(TScriptPackContext));
            }
            return _settings;
        }
    }
}
