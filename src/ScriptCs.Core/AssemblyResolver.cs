using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Common.Logging;

using ServiceStack.Text;

namespace ScriptCs
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly IFileSystem _fileSystem;

        private readonly ILog _logger;

        public AssemblyResolver(IFileSystem fileSystem, ILog logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public IEnumerable<string> GetAssemblyPaths(string path)
        {
            Guard.AgainstNullArgument("path", path);

            var manifestAssemblies = GetManifestAssemblies(path);
            var looseAssemblies = GetLooseAssemblies(path);

            var assemblyPaths = manifestAssemblies.Union(looseAssemblies).ToList();
            
            foreach (var assemblyPath in assemblyPaths)
            {
                _logger.DebugFormat("Found assembly reference: {0}", Path.GetFileName(assemblyPath));
            }

            return assemblyPaths;
        }

        private IEnumerable<string> GetLooseAssemblies(string path)
        {
            var binFolder = Path.Combine(path, Constants.BinFolder);
            if (!_fileSystem.DirectoryExists(binFolder)) 
                return Enumerable.Empty<string>();

            var looseAssemblies = _fileSystem.EnumerateFiles(binFolder, "*.dll")
                .Union(_fileSystem.EnumerateFiles(binFolder, "*.exe"))
                .Where(IsManagedAssembly);

            return looseAssemblies;
        }

        private IEnumerable<string> GetManifestAssemblies(string path)
        {
            var manifestPath = Path.Combine(path, Constants.ManifestFile);
            if (!_fileSystem.FileExists(manifestPath))
                return Enumerable.Empty<string>();

            var manifest = _fileSystem.ReadFile(manifestPath).FromJson<ScriptManifest>();

            return manifest.PackageAssemblies;
        }

        private static bool IsManagedAssembly(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
        }
    }
}
