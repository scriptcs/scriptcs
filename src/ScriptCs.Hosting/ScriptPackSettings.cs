using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackSettings : IScriptPackSettings
    {
        private IList<string> _references;
        private IList<string> _imports;
        private Type _contextType;

        public ScriptPackSettings(Type contextType)
        {
            _contextType = contextType;
        }

        public IEnumerable<string> GetReferences()
        {
            return _references;
        }

        public IEnumerable<string> GetImports()
        {
            return _imports;
        }

        public Type GetContextType()
        {
            return _contextType;
        }

        void IScriptPackSettingsImports.Imports(params string[] imports)
        {
            _imports = imports.ToList();
        }

        IScriptPackSettingsImports IScriptPackSettingsReferences.References(params string[] references)
        {
            _references = references.ToList();
            return this;
        }
    }


}