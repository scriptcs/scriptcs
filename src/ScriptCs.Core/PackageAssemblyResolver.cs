using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs
{
    public class PackageAssemblyResolver : IPackageAssemblyResolver
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPackageContainer _packageContainer;
        private readonly ILog _logger;
        private List<IPackageReference> _topLevelPackages;

        public PackageAssemblyResolver(IFileSystem fileSystem, IPackageContainer packageContainer, ILog logger)
        {
            _fileSystem = fileSystem;
            _packageContainer = packageContainer;
            _logger = logger;
        }

        public void SavePackages()
        {
            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFolder);

            if (!_fileSystem.DirectoryExists(packagesFolder))
            {
                _logger.Info("Packages directory does not exist!");
                return;
            }

            _packageContainer.CreatePackageFile();
        }

        public IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packages = _packageContainer.FindReferences(packageFile).ToList();

            _topLevelPackages = packages;

            return packages;
        }

        public IEnumerable<string> GetAssemblyNames(string workingDirectory)
        {
            var packages = GetPackages(workingDirectory).ToList();
            if (!packages.Any())
            {
                return Enumerable.Empty<string>();
            }

            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packageDir = Path.Combine(workingDirectory, Constants.PackagesFolder);

            var foundAssemblyPaths = new List<string>();
            var missingAssemblies = new List<IPackageReference>();

            LoadFiles(packageDir, packages, missingAssemblies, foundAssemblyPaths, _fileSystem.FileExists(packageFile));

            if (missingAssemblies.Count > 0)
            {
                var missingAssembliesString = string.Join(",", missingAssemblies.Select(i => i.PackageId + " " + i.FrameworkName.FullName));
                throw new MissingAssemblyException(string.Format("Missing: {0}", missingAssembliesString));
            }

            return foundAssemblyPaths;
        }

        private void LoadFiles(string packageDir, IEnumerable<IPackageReference> packageReferences, List<IPackageReference> missingAssemblies, List<string> foundAssemblies, bool strictLoad = true)
        {
            foreach (var packageRef in packageReferences)
            {
                var nugetPackage = _packageContainer.FindPackage(packageDir, packageRef);
                if (nugetPackage == null)
                {
                    missingAssemblies.Add(packageRef);
                    _logger.Info("Cannot find: " + packageRef.PackageId + " " + packageRef.Version);

                    continue;
                }

                var compatibleFiles = nugetPackage.GetCompatibleDlls(packageRef.FrameworkName);
                if (compatibleFiles == null)
                {
                    missingAssemblies.Add(packageRef);
                    _logger.Info("Cannot find binaries for " + packageRef.FrameworkName + " in: " + packageRef.PackageId + " " + packageRef.Version);

                    continue;
                }

                var compatibleFilePaths = compatibleFiles.Select(packageFile => Path.Combine(packageDir, nugetPackage.FullName, packageFile));

                foreach (var path in compatibleFilePaths)
                {
                    if (foundAssemblies.Contains(path))
                    {
                        continue;
                    }

                    foundAssemblies.Add(path);
                    _logger.Debug("Found: " + path);
                }

                if (nugetPackage.Dependencies == null || !nugetPackage.Dependencies.Any() || !strictLoad)
                {
                    continue;
                }

                var dependencyReferences = nugetPackage.Dependencies
                    .Where(i => _topLevelPackages.All(x => x.PackageId != i.Id))
                    .Select(i => new PackageReference(i.Id, i.FrameworkName, i.Version));

                LoadFiles(packageDir, dependencyReferences, missingAssemblies, foundAssemblies, true);
            }
        }
    }
}