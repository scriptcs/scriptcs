using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace ScriptCs
{
    public class ScriptPackManager : IScriptPackManager
    {
        private readonly CompositionContainer _container;

        public ScriptPackManager(CompositionContainer container)
        {
            _container = container;
        }

        public IEnumerable<IScriptPack> GetPacks()
        {
            return _container.GetExportedValues<IScriptPack>();
        }
    }
}