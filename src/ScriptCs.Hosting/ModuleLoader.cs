using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ModuleLoader : IModuleLoader
    {
        internal static readonly Dictionary<string, string> DefaultCSharpModules = new Dictionary<string, string>
        {
            {"roslyn", "ScriptCs.Engine.Roslyn.dll"},
            {"mono", "ScriptCs.Engine.Mono.dll"}
        };

        internal static readonly string DefaultCSharpExtension = ".csx";

        private readonly IAssemblyResolver _resolver;
        private readonly ILog _logger;
        private readonly Action<Assembly, AggregateCatalog> _addToCatalog;
        private readonly Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> _getLazyModules;
        private readonly IFileSystem _fileSystem;
        private readonly IAssemblyUtility _assemblyUtility;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver, Common.Logging.ILog logger, IFileSystem fileSystem, IAssemblyUtility assemblyUtility)
            : this(resolver, new CommonLoggingLogProvider(logger), fileSystem, assemblyUtility)
        {
        }

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public ModuleLoader(IAssemblyResolver resolver, Common.Logging.ILog logger, Action<Assembly, AggregateCatalog> addToCatalog, Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> getLazyModules, IFileSystem fileSystem, IAssemblyUtility assemblyUtility)
            : this(resolver, new CommonLoggingLogProvider(logger), addToCatalog, getLazyModules, fileSystem, assemblyUtility)
        {
        }

        [ImportingConstructor]
        public ModuleLoader(IAssemblyResolver resolver, ILogProvider logProvider, IFileSystem fileSystem, IAssemblyUtility assemblyUtility) :
            this(resolver, logProvider, null, null, fileSystem, assemblyUtility)
        {
        }

        public ModuleLoader(IAssemblyResolver resolver, ILogProvider logProvider, Action<Assembly, AggregateCatalog> addToCatalog, Func<CompositionContainer, IEnumerable<Lazy<IModule, IModuleMetadata>>> getLazyModules, IFileSystem fileSystem, IAssemblyUtility assemblyUtility)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _resolver = resolver;
            _logger = logProvider.ForCurrentType();

            if (addToCatalog == null)
            {
                addToCatalog = (assembly, catalog) =>
                {
                    try
                    {
                        var assemblyCatalog = new AssemblyCatalog(assembly);
                        catalog.Catalogs.Add(assemblyCatalog);
                    }
                    catch (Exception exception)
                    {
                        _logger.DebugFormat("Module Loader exception: {0}", exception.Message);
                    }
                };
            }

            _addToCatalog = addToCatalog;

            if (getLazyModules == null)
            {
                getLazyModules = container => container.GetExports<IModule, IModuleMetadata>();
            }

            _getLazyModules = getLazyModules;
            _fileSystem = fileSystem;
            _assemblyUtility = assemblyUtility;
        }

        public void Load(IModuleConfiguration config, string[] modulePackagesPaths, string hostBin, string extension,
            params string[] moduleNames)
        {
            Guard.AgainstNullArgument("moduleNames", moduleNames);

            if (modulePackagesPaths == null) return;

            // only CSharp module needed - use fast path
            if (moduleNames.Length == 1 && DefaultCSharpModules.ContainsKey(moduleNames[0]) && (string.IsNullOrWhiteSpace(extension) || extension.Equals(DefaultCSharpExtension, StringComparison.InvariantCultureIgnoreCase))) 
            {
                _logger.Debug("Only CSharp module is needed - will skip module lookup");
                var csharpModuleAssembly = DefaultCSharpModules[moduleNames[0]];
                var module = _assemblyUtility.LoadFile(Path.Combine(hostBin, csharpModuleAssembly));

                if (module != null)
                {
                    var moduleType = module.GetExportedTypes().FirstOrDefault(f => typeof (IModule).IsAssignableFrom(f));

                    if (moduleType != null)
                    {
                        var moduleInstance = Activator.CreateInstance(moduleType) as IModule;

                        if (moduleInstance != null)
                        {
                            _logger.Debug(String.Format("Initializing module: {0}", module.GetType().FullName));
                            moduleInstance.Initialize(config);
                            return;
                        }
                    }
                }
            }

            _logger.Debug("Loading modules from: " + String.Join(", ", modulePackagesPaths.Select(i => i)));
            var paths = new List<string>();

            AddPaths(modulePackagesPaths, hostBin, paths);

            var catalog = CreateAggregateCatalog(paths);
            var container = new CompositionContainer(catalog);

            var lazyModules = GetLazyModules(container);
            InitializeModules(config, extension, moduleNames, lazyModules);
        }

        private void InitializeModules(
            IModuleConfiguration config,
            string extension,
            IEnumerable<string> moduleNames,
            IEnumerable<Lazy<IModule, IModuleMetadata>> lazyModules)
        {
            var modules = lazyModules
                .Where(m => moduleNames.Contains(m.Metadata.Name) ||
                            (extension != null && m.Metadata.Extensions != null &&
                                (m.Metadata.Extensions.Split(',').Contains(extension))) ||
                            m.Metadata.Autoload)
                .Select(m => m.Value);

            _logger.Debug("Initializing modules");

            foreach (var module in modules)
            {
                _logger.Debug(String.Format("Initializing module: {0}", module.GetType().FullName));
                module.Initialize(config);
            }

            _logger.Debug("Modules initialized");
        }

        private AggregateCatalog CreateAggregateCatalog(IEnumerable<string> paths)
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

        private void AddPaths(IEnumerable<string> modulePackagesPaths, string hostBin, List<string> paths)
        {
            foreach (var modulePaths in modulePackagesPaths
                .Select(modulePackagesPath => _resolver.GetAssemblyPaths(modulePackagesPath, true)))
            {
                paths.AddRange(modulePaths);
            }

            if (hostBin != null)
            {
                var assemblyPaths = _fileSystem.EnumerateBinaries(hostBin, SearchOption.TopDirectoryOnly);
                paths.AddRange(assemblyPaths);
            }
        }

        private IEnumerable<Lazy<IModule, IModuleMetadata>> GetLazyModules(CompositionContainer container)
        {
            IEnumerable<Lazy<IModule, IModuleMetadata>> lazyModules;

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
