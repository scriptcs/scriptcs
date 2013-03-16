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
        private IEnumerable<IPackageReference> _topLevelPackages;

        public PackageAssemblyResolver(IFileSystem fileSystem, IPackageContainer packageContainer)
        {
            _fileSystem = fileSystem;
            _packageContainer = packageContainer;
        }

        public IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packages = _packageContainer.FindReferences(packageFile);
            _topLevelPackages = packages;

            return packages;
        }

        public IEnumerable<string> GetAssemblyNames(string workingDirectory, Action<string> outputCallback = null)
        {
            var packages = GetPackages(workingDirectory);
            if (!packages.Any())
            {
                return Enumerable.Empty<string>();
            }

            var packageFile = Path.Combine(workingDirectory, Constants.PackagesFile);
            var packageDir = Path.Combine(workingDirectory, Constants.PackagesFolder);

            var foundAssemblyPaths = new List<string>();
            var missingAssemblies = new List<IPackageReference>();

            LoadFiles(packageDir, packages, ref missingAssemblies, ref foundAssemblyPaths, _fileSystem.FileExists(packageFile), outputCallback);

            if (missingAssemblies.Count > 0)
                throw new MissingAssemblyException(string.Format("Missing: {0}", string.Join(",", missingAssemblies.Select(i => i.PackageId + " " + i.FrameworkName.FullName))));

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

                foreach (var packageFile in compatibleFiles)
                {
                    var path = Path.Combine(packageDir, nugetPackage.FullName, packageFile);
                    if (!foundAssemblies.Contains(path))
                    {
                        foundAssemblies.Add(path);
                        if (outputCallback != null)
                        {
                            outputCallback("Found: " + path);
                        }
                    }
                }

                if (nugetPackage.Dependencies != null && nugetPackage.Dependencies.Any() && strictLoad)
                {
                    LoadFiles(packageDir,
                        nugetPackage.Dependencies.Where(i => _topLevelPackages.All(x => x.PackageId != i.Id)).Select(
                                  i => new PackageReference(i.Id, i.FrameworkName, i.Version)), ref missingAssemblies,
                              ref foundAssemblies, true, outputCallback);
                }
            }
        }
    }
}