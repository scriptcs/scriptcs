using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Common.Logging;

namespace ScriptCs
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly IFileSystem _fileSystem;

        private readonly ILog _logger;

        public AssemblyResolver(IPackageAssemblyResolver packageAssemblyResolver, IFileSystem fileSystem, ILog logger)
        {
            _packageAssemblyResolver = packageAssemblyResolver;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public IEnumerable<string> GetAssemblyPaths(string path)
        {
            var assemblyPaths = new List<string>();

            var packagesFolder = Path.Combine(path, Constants.PackagesFolder);

            if (_fileSystem.DirectoryExists(packagesFolder))
            {
                var packageAssemblies = _packageAssemblyResolver.GetAssemblyNames(path);
                assemblyPaths.AddRange(packageAssemblies);
            }

            var looseAssemblies = _fileSystem.EnumerateFiles(path, "*.dll", SearchOption.TopDirectoryOnly)
                    .Union(_fileSystem.EnumerateFiles(path, "*.exe", SearchOption.TopDirectoryOnly))
                    .Where(IsManagedAssembly);

            assemblyPaths.AddRange(looseAssemblies);

            foreach (var assemblyPath in assemblyPaths)
            {
                _logger.DebugFormat("Found assembly reference: {0}", Path.GetFileName(assemblyPath));
            }

            return assemblyPaths;
        }

        private static bool IsManagedAssembly(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
            }
            catch (BadImageFormatException)
            {
                return false;
            }
            return true;
        }
    }
}
