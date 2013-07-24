using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public class ModuleLoader
    {
        private readonly IAssemblyResolver _resolver;
        private readonly Action<string, AggregateCatalog> _catalogAction;

        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver) :
            this(resolver, null)
        {         
        }

        public ModuleLoader(IAssemblyResolver resolver, Action<string, AggregateCatalog> catalogAction)
        {
            _resolver = resolver;
            _catalogAction = catalogAction;
            if (catalogAction == null)
                catalogAction = (p, catalog) => catalog.Catalogs.Add(new AssemblyCatalog(p));

            _catalogAction = catalogAction;
        }

        public void Load(IScriptRuntimeBuilder builder, string modulePackagesPath, string extension, bool repl, bool debug, string[] moduleNames)
        {
            var paths = _resolver.GetAssemblyPaths(modulePackagesPath);
            var catalog = new AggregateCatalog();
            foreach (var path in paths)
            {
                _catalogAction(path, catalog);
            }

            var container = new CompositionContainer(catalog);
            var modules = container.GetExports<IModule, IModuleMetadata>()
                .Where(m => moduleNames.Contains(m.Metadata.Name) ||
                    (extension != null && (m.Metadata.Extensions == extension || m.Metadata.Extensions.Split(',').Contains(extension))))
                .Select(m => m.Value);

            foreach (var module in modules)
            {
                module.Initialize(builder, repl, debug);
            }
        }
    }
}
