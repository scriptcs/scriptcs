using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace ScriptCs
{
    public class ScriptPackResolver : IScriptPackResolver
    {
        private readonly ExportProvider _exportProvider;
        private IEnumerable<IScriptPack> _scriptPacks; 
 
        public ScriptPackResolver(ExportProvider exportProvider)
        {
            _exportProvider = exportProvider;
        }

        public IEnumerable<IScriptPack> GetPacks()
        {
            if (_scriptPacks == null)
            {
                _scriptPacks = _exportProvider.GetExportedValues<IScriptPack>();
            }
            return _scriptPacks;
        }
    }
}