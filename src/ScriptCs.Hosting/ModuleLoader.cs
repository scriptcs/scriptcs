using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ModuleLoader : IModuleLoader
    {
        private readonly IAssemblyResolver _resolver;
        private readonly ILog _logger;
        private readonly Action<Assembly, AggregateCatalog> _addToCatalog;
        private readonly Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getLazyModules;
        private readonly IFileSystem _fileSystem;
        private readonly IAssemblyUtility _assemblyUtility;

        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver, ILog logger, IFileSystem fileSystem, IAssemblyUtility assemblyUtility) :
            this(resolver, logger, null, null, fileSystem, assemblyUtility )
        {         
        }

        public ModuleLoader(IAssemblyResolver resolver, ILog logger, Action<Assembly, AggregateCatalog> addToCatalog, Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> getLazyModules, IFileSystem fileSystem, IAssemblyUtility assemblyUtility)
        {
            _resolver = resolver;
            _logger = logger;

            if (addToCatalog == null)
            {
                addToCatalog = (assembly, catalog) =>
                {
                    try
                    {
                        var assemblyCatalog = new AssemblyCatalog(assembly);
                        catalog.Catalogs.Add(assemblyCatalog);
                    }
                    catch(Exception exception)
                    {
                        logger.DebugFormat("Module Loader exception: {0}", exception.Message);
                    }
                };
            }

            _addToCatalog = addToCatalog;

            if (getLazyModules == null)
            {
                getLazyModules = (container) =>
                {
                    return container.GetExports<IModule, IModuleMetadata>();
                };
            }
            _getLazyModules = getLazyModules;
            _fileSystem = fileSystem;
            _assemblyUtility = assemblyUtility;
        }

        public void Load(IModuleConfiguration config, string[] modulePackagesPaths, string hostBin, string extension, params string[] moduleNames)
        {
            if (modulePackagesPaths == null) return;

            _logger.Debug("Loading modules from: " + string.Join(", ", modulePackagesPaths.Select(i => i)));
            var paths = new List<string>();

            AddPaths(modulePackagesPaths, hostBin, paths);

            var catalog = CreateAggregateCatalog(paths);
            var container = new CompositionContainer(catalog);

            var lazyModules = GetLazyModules(container);
            InitializeModules(config, extension, moduleNames, lazyModules);
        }

        private void InitializeModules(IModuleConfiguration config, string extension, string[] moduleNames,
            IEnumerable<Lazy<IModule, IModuleMetadata>> lazyModules)
        {
            var modules = lazyModules
                .Where(m => moduleNames.Contains(m.Metadata.Name) ||
                            (extension != null && m.Metadata.Extensions != null &&
                             (m.Metadata.Extensions.Split(',').Contains(extension))) || m.Metadata.Autoload == true)
                .Select(m => m.Value);

            _logger.Debug("Initializing modules");

            foreach (var module in modules)
            {
                _logger.Debug(string.Format("Initializing module: {0}", module.GetType().FullName));
                module.Initialize(config);
            }

            _logger.Debug("Modules initialized");
        }

        private AggregateCatalog CreateAggregateCatalog(List<string> paths)
        {
            var catalog = new AggregateCatalog();
            foreach (var path in paths)
            {
                _logger.DebugFormat("Found assembly: {0}", path);

                try
                {
                    if (_assemblyUtility.IsManagedAssembly(path))
                    {
                        _logger.DebugFormat("Adding Assembly: {0} to catalog", path);
                        var name = _assemblyUtility.GetAssemblyName(path);
                        var assembly = _assemblyUtility.Load(name);
                        _addToCatalog(assembly, catalog);
                    }
                    else
                    {
                        _logger.DebugFormat("Skipping Adding Native Assembly {0} to catalog", path);
                    }
                }
                catch (Exception exception)
                {
                    _logger.DebugFormat("Module Loader exception: {0}", exception.Message);
                }
            }
            return catalog;
        }

        private void AddPaths(string[] modulePackagesPaths, string hostBin, List<string> paths)
        {
            foreach (var modulePackagesPath in modulePackagesPaths)
            {
                var modulePaths = _resolver.GetAssemblyPaths(modulePackagesPath, true);
                paths.AddRange(modulePaths);
            }

            if (hostBin != null)
            {
                var assemblyPaths = _fileSystem.EnumerateBinaries(hostBin, SearchOption.TopDirectoryOnly);
                foreach (var path in assemblyPaths)
                {
                    paths.Add(path);
                }
            }
        }

        private IEnumerable<Lazy<IModule, IModuleMetadata>> GetLazyModules(CompositionContainer container)
        {
            IEnumerable<Lazy<IModule, IModuleMetadata>> lazyModules = null;

            try
            {
                lazyModules = _getLazyModules(container);
            }
            catch (ReflectionTypeLoadException exception)
            {
                if (exception.LoaderExceptions != null && exception.LoaderExceptions.Any())
                {
                    foreach (var loaderException in exception.LoaderExceptions)
                    {
                        _logger.DebugFormat("Module Loader exception:  {0}", loaderException.Message);
                    }
                }
                else
                {
                    _logger.DebugFormat("Module Loader exception:  {0}", exception.Message);
                }
                lazyModules = Enumerable.Empty<Lazy<IModule, IModuleMetadata>>();
            }
            return lazyModules;
        }
    }
}
