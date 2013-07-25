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
        private readonly Action<string, AggregateCatalog> _addToCatalog;
        private readonly Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getModules;

        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver) :
            this(resolver, null, null)
        {         
        }

        public ModuleLoader(IAssemblyResolver resolver, Action<string, AggregateCatalog> addToCatalog, Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> getModules)
        {
            _resolver = resolver;
            if (addToCatalog == null)
                addToCatalog = (p, catalog) => catalog.Catalogs.Add(new AssemblyCatalog(p));

            _addToCatalog = addToCatalog;

            if (getModules == null)
                getModules = (container) => container.GetExports<IModule, IModuleMetadata>();

            _getModules = getModules;

        }

        public void Load(IScriptRuntimeBuilder builder, string modulePackagesPath, string extension, bool repl, bool debug, params string[] moduleNames)
        {
            var paths = _resolver.GetAssemblyPaths(modulePackagesPath);
            var catalog = new AggregateCatalog();
            foreach (var path in paths)
            {
                _addToCatalog(path, catalog);
            }

            var container = new CompositionContainer(catalog);
            var lazyModules = _getModules(container);
            var modules = lazyModules
                .Where(m => moduleNames.Contains(m.Metadata.Name) ||
                    (extension != null && m.Metadata.Extensions != null && (m.Metadata.Extensions.Equals(extension) || m.Metadata.Extensions.Split(',').Contains(extension))))
                .Select(m => m.Value);

            foreach (var module in modules)
            {
                module.Initialize(builder, repl, debug);
            }
        }
    }
}
