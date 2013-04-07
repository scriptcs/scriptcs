using System;
using System.Collections.Generic;
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

        private List<IPackageReference> _topLevelPackages;

        public PackageAssemblyResolver(IFileSystem fileSystem, IPackageContainer packageContainer)
        {
            _fileSystem = fileSystem;
            _packageContainer = packageContainer;
        }

        public void SavePackages(Action<string> output)
        {
            var packagesFile = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFile);
            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFolder);

            if (_fileSystem.FileExists(packagesFile))
            {
                output("Packages.config already exists!");
                return;
            }

            if (!_fileSystem.DirectoryExists(packagesFolder))
            {
                output("Packages directory does not exist!");
                return;
            }

            var result = _packageContainer.CreatePackageFile().ToList();
            if (!result.Any())
            {
                output("No packages found!");
            }

            result.ForEach(i => output(string.Format("Added {0}", i)));
            output("Packages.config successfully created!");
        }

        public IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packages = _packageContainer.FindReferences(packageFile).ToList();

            _topLevelPackages = packages;

            return packages;
        }

        public IEnumerable<string> GetAssemblyNames(string workingDirectory, Action<string> outputCallback = null)
        {
            var packages = GetPackages(workingDirectory).ToList();
            if (!packages.Any()) return Enumerable.Empty<string>();

            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packageDir = Path.Combine(workingDirectory, Constants.PackagesFolder);

            var foundAssemblyPaths = new List<string>();
            var missingAssemblies = new List<IPackageReference>();

            LoadFiles(packageDir, packages, ref missingAssemblies, ref foundAssemblyPaths, _fileSystem.FileExists(packageFile), outputCallback);

            if (missingAssemblies.Count > 0)
            {
                var missingAssembliesString = string.Join(",", missingAssemblies.Select(i => i.PackageId + " " + i.FrameworkName.FullName));
                throw new MissingAssemblyException(string.Format("Missing: {0}", missingAssembliesString));
            }

            return foundAssemblyPaths;
        }

        private void LoadFiles(string packageDir, IEnumerable<IPackageReference> packageReferences, ref List<IPackageReference> missingAssemblies, ref List<string> foundAssemblies, bool strictLoad = true, Action<string> outputCallback = null)
        {
            foreach (var packageRef in packageReferences)
            {
                var nugetPackage = _packageContainer.FindPackage(packageDir, packageRef);
                if (nugetPackage == null)
                {
                    missingAssemblies.Add(packageRef);
                    if (outputCallback != null)
                    {
                        outputCallback("Cannot find: " + packageRef.PackageId + " " + packageRef.Version);
                    }

                    continue;
                }

                var compatibleFiles = nugetPackage.GetCompatibleDlls(packageRef.FrameworkName);
                if (compatibleFiles == null)
                {
                    missingAssemblies.Add(packageRef);
                    if (outputCallback != null)
                    {
                        outputCallback("Cannot find binaries for " + packageRef.FrameworkName + " in: " +
                                       packageRef.PackageId + " " + packageRef.Version);
                    }

                    continue;
                }

                var compatibleFilePaths = compatibleFiles.Select(packageFile => Path.Combine(packageDir, nugetPackage.FullName, packageFile));

                foreach (var path in compatibleFilePaths)
                {
                    if (foundAssemblies.Contains(path)) continue;

                    foundAssemblies.Add(path);
                    if (outputCallback != null)
                    {
                        outputCallback("Found: " + path);
                    }
                }

                if (nugetPackage.Dependencies == null || !nugetPackage.Dependencies.Any() || !strictLoad) continue;

                var dependencyReferences = nugetPackage.Dependencies
                    .Where(i => _topLevelPackages.All(x => x.PackageId != i.Id))
                    .Select(i => new PackageReference(i.Id, i.FrameworkName, i.Version));

                LoadFiles(packageDir, dependencyReferences, ref missingAssemblies, ref foundAssemblies, true, outputCallback);
            }
        }
    }
}