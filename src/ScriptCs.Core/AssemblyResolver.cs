using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, List<string>> _assemblyPathCache = new Dictionary<string, List<string>>();
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IAssemblyUtility _assemblyUtility;
        private readonly ILog _logger;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public AssemblyResolver(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IAssemblyUtility assemblyUtility,
            Common.Logging.ILog logger)
            :this(fileSystem,packageAssemblyResolver,assemblyUtility,new CommonLoggingLogProvider(logger))
        {
        }

        public AssemblyResolver(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IAssemblyUtility assemblyUtility,
            ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgumentProperty("fileSystem", "PackagesFolder", fileSystem.PackagesFolder);
            Guard.AgainstNullArgumentProperty("fileSystem", "BinFolder", fileSystem.BinFolder);

            Guard.AgainstNullArgument("packageAssemblyResolver", packageAssemblyResolver);
            Guard.AgainstNullArgument("assemblyUtility", assemblyUtility);
            Guard.AgainstNullArgument("logProvider", logProvider);

            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _assemblyUtility = assemblyUtility;
            _logger = logProvider.ForCurrentType();
        }

        public IEnumerable<string> GetAssemblyPaths(string path, bool binariesOnly = false)
        {
            Guard.AgainstNullArgument("path", path);

            List<string> assemblies;
            if (!_assemblyPathCache.TryGetValue(path, out assemblies))
            {
                assemblies = GetPackageAssemblyNames(path).Union(GetBinAssemblyPaths(path)).ToList();
                _assemblyPathCache.Add(path, assemblies);
            }
           
            return binariesOnly
                ? assemblies.Where(m =>
                    m.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ||
                    m.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                : assemblies.ToArray();
        }

        private IEnumerable<string> GetBinAssemblyPaths(string path)
        {
            var binFolder = Path.Combine(path, _fileSystem.BinFolder);
            if (!_fileSystem.DirectoryExists(binFolder))
            {
                yield break;
            }

            foreach (var assembly in _fileSystem.EnumerateBinaries(binFolder, SearchOption.TopDirectoryOnly)
                .Where(f => _assemblyUtility.IsManagedAssembly(f)))
            {
                _logger.DebugFormat("Found assembly in bin folder: {0}", Path.GetFileName(assembly));
                yield return assembly;
            }
        }

        private IEnumerable<string> GetPackageAssemblyNames(string path)
        {
            foreach (var assembly in _packageAssemblyResolver.GetAssemblyNames(path))
            {
                _logger.DebugFormat("Found package assembly: {0}", Path.GetFileName(assembly));
                yield return assembly;
            }
        }
    }
}
