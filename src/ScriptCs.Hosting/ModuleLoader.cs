using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ModuleLoader : IModuleLoader
    {
        private readonly IAssemblyResolver _resolver;
        private readonly ILog _logger;
        private readonly Action<string, AggregateCatalog> _addToCatalog;
        private readonly Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getModules;
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver, ILog logger, IFileSystem fileSystem) :
            this(resolver, logger, null, null, fileSystem)
        {         
        }

        public ModuleLoader(IAssemblyResolver resolver, ILog logger, Action<string, AggregateCatalog> addToCatalog, Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> getModules, IFileSystem fileSystem)
        {
            _resolver = resolver;
            _logger = logger;
            if (addToCatalog == null)
            {
                addToCatalog = (p, catalog) =>
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(p);
                        catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                    }
                    catch(Exception exception)
                    {
                        logger.DebugFormat("Module Loader exception: {0}", exception.Message);
                    }
                };
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
                                logger.DebugFormat("Module Loader exception:  {0}", loaderException.Message);
                            }
                        }
                        else
                        {
                            logger.DebugFormat("Module Loader exception:  {0}", exception.Message);
                        }
                        return Enumerable.Empty<Lazy<IModule, IModuleMetadata>>();
                    }
                };

            }
            _getModules = getModules;
            _fileSystem = fileSystem;
        }

        public void Load(IModuleConfiguration config, string[] modulePackagesPaths, string hostBin, string extension, params string[] moduleNames)
        {
            if (modulePackagesPaths == null) return;

            _logger.Debug("Loading modules from: " + string.Join(", ", modulePackagesPaths.Select(i => i)));
            var paths = new List<string>();
            foreach (var modulePackagesPath in modulePackagesPaths)
            {
                var modulePaths = _resolver.GetAssemblyPaths(modulePackagesPath);
                paths.AddRange(modulePaths);
            }

            if (hostBin != null)
            {
                var assemblyPaths = _fileSystem.EnumerateFiles(hostBin, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (var path in assemblyPaths)
                {
                    paths.Add(path);
                }
            }

            var catalog = new AggregateCatalog();
            foreach (var path in paths)
            {
                _logger.DebugFormat("Found assembly: {0}", path);
                _addToCatalog(path, catalog);
            }

            var container = new CompositionContainer(catalog);
            var lazyModules = _getModules(container);
            var modules = lazyModules
                .Where(m => moduleNames.Contains(m.Metadata.Name) || 
                    (extension != null && m.Metadata.Extensions != null && (m.Metadata.Extensions.Split(',').Contains(extension))) || m.Metadata.Autoload == true) 
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
