using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ModuleLoader : IModuleLoader
    {
        private readonly IAssemblyResolver _resolver;
        private readonly ILog _logger;
        private readonly Action<string, AggregateCatalog> _addToCatalog;
        private readonly Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getModules;

        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver, ILog logger) :
            this(resolver, logger, null, null)
        {         
        }

        public ModuleLoader(IAssemblyResolver resolver, ILog logger, Action<string, AggregateCatalog> addToCatalog, Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> getModules)
        {
            _resolver = resolver;
            _logger = logger;
            if (addToCatalog == null)
            {
                addToCatalog = (p, catalog) => catalog.Catalogs.Add(new AssemblyCatalog(p));
            }

            _addToCatalog = addToCatalog;

            if (getModules == null)
            {
                getModules = (container) =>
                {
                    try
                    {
                        return container.GetExports<IModule, IModuleMetadata>();
                    }
                    catch (ReflectionTypeLoadException exception)
                    {
                        if (exception.LoaderExceptions != null && exception.LoaderExceptions.Any())
                        {
                            foreach (var loaderException in exception.LoaderExceptions)
                            {
                                logger.Error(string.Format("Module loader exception {0}", loaderException.Message));
                            }
                        }
                        else
                        {
                            logger.Error("Module loader threw an exception", exception);
                        }
                        return Enumerable.Empty<Lazy<IModule, IModuleMetadata>>();
                    }
                };

            }

            _getModules = getModules;
        }

        public void Load(IModuleConfiguration config, string[] modulePackagesPaths, string extension, params string[] moduleNames)
        {
            if (modulePackagesPaths == null) return;

            _logger.Debug("Loading modules from: " + string.Join(", ", modulePackagesPaths.Select(i => i)));
            var paths = new List<string>();
            foreach (var modulePackagesPath in modulePackagesPaths)
            {
                var modulePaths = _resolver.GetAssemblyPaths(modulePackagesPath);
                paths.AddRange(modulePaths);
            }

            var catalog = new AggregateCatalog();
            foreach (var path in paths)
            {
                _addToCatalog(path, catalog);
            }

            var container = new CompositionContainer(catalog);
            var lazyModules = _getModules(container);
            var modules = lazyModules
                .Where(m => moduleNames.Contains(m.Metadata.Name) ||
                    (extension != null && m.Metadata.Extensions != null && (m.Metadata.Extensions.Split(',').Contains(extension))))
                .Select(m => m.Value);

            _logger.Debug("Initializing modules");
            foreach (var module in modules)
            {
                _logger.Debug(string.Format("Initializing module: {0}", module.GetType().FullName));
                module.Initialize(config);
            }

            _logger.Debug("Modules initialized");
        }
    }
}
