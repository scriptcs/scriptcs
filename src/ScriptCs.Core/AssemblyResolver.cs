using System.Collections.Generic;
using System.IO;
using System.Linq;

using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, List<string>> _assemblyPathCache = new Dictionary<string, List<string>>();
 
        private readonly IFileSystem _fileSystem;

        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly ILog _logger;

        private readonly IAssemblyUtility _assemblyUtility;

        public AssemblyResolver(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IAssemblyUtility assemblyUtility,
            ILog logger)
        {
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _logger = logger;
            _assemblyUtility = assemblyUtility;
        }

        public IEnumerable<string> GetAssemblyPaths(string path)
        {
            Guard.AgainstNullArgument("path", path);

            List<string> assemblies;
            if (_assemblyPathCache.TryGetValue(path, out assemblies))
            {
                return assemblies;
            }

            var packageAssemblies = GetPackageAssemblies(path);
            var binAssemblies = GetBinAssemblies(path);

            assemblies = packageAssemblies.Union(binAssemblies).ToList();
            _assemblyPathCache.Add(path, assemblies);

            return assemblies;
        }

        private IEnumerable<string> GetBinAssemblies(string path)
        {
            var binFolder = Path.Combine(path, Constants.BinFolder);
            if (!_fileSystem.DirectoryExists(binFolder))
            {
                return Enumerable.Empty<string>();
            }

            var assemblies = _fileSystem.EnumerateFiles(binFolder, "*.dll")
                .Union(_fileSystem.EnumerateFiles(binFolder, "*.exe"))
                .Where(f => _assemblyUtility.IsManagedAssembly(f))
                .ToList();

            foreach (var assembly in assemblies)
            {
                _logger.DebugFormat("Found assembly in bin folder: {0}", Path.GetFileName(assembly));
            }

            return assemblies;
        }

        private IEnumerable<string> GetPackageAssemblies(string path)
        {
            var packagesFolder = Path.Combine(path, Constants.PackagesFolder);
            if (!_fileSystem.DirectoryExists(packagesFolder))
            {
                return Enumerable.Empty<string>();
            }

            var assemblies = _packageAssemblyResolver.GetAssemblyNames(path).ToList();

            foreach (var packageAssembly in assemblies)
            {
                _logger.DebugFormat("Found package assembly: {0}", Path.GetFileName(packageAssembly));
            }

            return assemblies;
        }
    }
}
