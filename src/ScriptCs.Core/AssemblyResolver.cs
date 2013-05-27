using System.Collections.Generic;
using System.IO;
using System.Linq;

using Common.Logging;

using ServiceStack.Text;

namespace ScriptCs
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly IFileSystem _fileSystem;

        private readonly ILog _logger;

        private readonly IAssemblyUtility _assemblyUtility;

        public AssemblyResolver(IFileSystem fileSystem, IAssemblyUtility assemblyUtility, ILog logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _assemblyUtility = assemblyUtility;
        }

        public IEnumerable<string> GetAssemblyPaths(string path)
        {
            Guard.AgainstNullArgument("path", path);

            var manifestAssemblies = GetManifestAssemblies(path);
            var looseAssemblies = GetLooseAssemblies(path);

            return manifestAssemblies.Union(looseAssemblies);
        }

        private IEnumerable<string> GetLooseAssemblies(string path)
        {
            var binFolder = Path.Combine(path, Constants.BinFolder);
            if (!_fileSystem.DirectoryExists(binFolder)) 
                return Enumerable.Empty<string>();

            var looseAssemblies = _fileSystem.EnumerateFiles(binFolder, "*.dll")
                .Union(_fileSystem.EnumerateFiles(binFolder, "*.exe"))
                .Where(_assemblyUtility.IsManagedAssembly)
                .ToList();

            foreach (var looseAssembly in looseAssemblies)
            {
                _logger.DebugFormat("Found assembly in bin folder: {0}", Path.GetFileName(looseAssembly));
            }

            return looseAssemblies;
        }

        private IEnumerable<string> GetManifestAssemblies(string path)
        {
            var manifestPath = Path.Combine(path, Constants.ManifestFile);
            if (!_fileSystem.FileExists(manifestPath))
                return Enumerable.Empty<string>();

            var manifest = _fileSystem.ReadFile(manifestPath).FromJson<ScriptManifest>();

            foreach (var packageAssembly in manifest.PackageAssemblies)
            {
                _logger.DebugFormat("Found package assembly: {0}", Path.GetFileName(packageAssembly));
            }

            return manifest.PackageAssemblies;
        }
    }
}
