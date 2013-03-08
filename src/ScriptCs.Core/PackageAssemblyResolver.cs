using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using ScriptCs.Exceptions;
using ScriptCs.Package;

namespace ScriptCs
{
    public class PackageAssemblyResolver : IPackageAssemblyResolver
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPackageContainer _packageContainer;

        [ImportingConstructor]
        public PackageAssemblyResolver(IFileSystem fileSystem, IPackageContainer packageContainer)
        {
            _fileSystem = fileSystem;
            _packageContainer = packageContainer;
        }

        public IEnumerable<string> GetAssemblyNames(string workingDirectory)
        {
            var packageDir = Path.Combine(workingDirectory, Constants.PackagesFolder);
            if (!_fileSystem.DirectoryExists(packageDir) || !_fileSystem.FileExists(Path.Combine(packageDir, Constants.PackagesFile)))
                return Enumerable.Empty<string>();

            var packages = _packageContainer.FindReferences(packageDir);
            var foundAssemblyPaths = new List<string>();
            var missingAssemblies = new List<IPackageReference>();

            foreach (var package in packages)
            {
                var nugetPackage = _packageContainer.FindPackage(packageDir, package.PackageId);
                if (nugetPackage == null)
                {
                    missingAssemblies.Add(package);
                    continue;
                }

                var compatibleFiles = nugetPackage.GetCompatibleDlls(package.FrameworkName);
                if (compatibleFiles == null)
                {
                    missingAssemblies.Add(package);
                    continue;
                }

                foreach (var packageFile in compatibleFiles) {
                    var path = Path.Combine(packageDir, nugetPackage.FullName, packageFile);
                    foundAssemblyPaths.Add(path);
                }
            }

            if (missingAssemblies.Count > 0)
                throw new MissingAssemblyException(string.Format("Missing: {0}", string.Join(",", missingAssemblies.Select(i => i.PackageId + " " + i.FrameworkName.FullName))));

            return foundAssemblyPaths;
        }
    }
}