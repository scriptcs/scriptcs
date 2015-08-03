using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class PackageAssemblyResolver : IPackageAssemblyResolver
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPackageContainer _packageContainer;
        private readonly ILog _logger;
        private readonly IAssemblyUtility _assemblyUtility;

        private List<IPackageReference> _topLevelPackages;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public PackageAssemblyResolver(
            IFileSystem fileSystem, IPackageContainer packageContainer, Common.Logging.ILog logger, IAssemblyUtility assemblyUtility)
            : this(fileSystem, packageContainer, new CommonLoggingLogProvider(logger), assemblyUtility)
        {
        }

        public PackageAssemblyResolver(
            IFileSystem fileSystem, IPackageContainer packageContainer, ILogProvider logProvider, IAssemblyUtility assemblyUtility)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgumentProperty("fileSystem", "PackagesFolder", fileSystem.PackagesFolder);
            Guard.AgainstNullArgumentProperty("fileSystem", "PackagesFile", fileSystem.PackagesFile);

            Guard.AgainstNullArgument("packageContainer", packageContainer);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("assemblyUtility", assemblyUtility);

            _fileSystem = fileSystem;
            _packageContainer = packageContainer;
            _logger = logProvider.ForCurrentType();
            _assemblyUtility = assemblyUtility;
        }

        public void SavePackages()
        {
            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            if (!_fileSystem.DirectoryExists(packagesFolder))
            {
                _logger.Warn("Packages directory does not exist!");
                return;
            }

            _packageContainer.CreatePackageFile();
        }

        public IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            var packagesFile = Path.Combine(workingDirectory, _fileSystem.PackagesFile);
            _topLevelPackages = _packageContainer.FindReferences(packagesFile).ToList();
            return _topLevelPackages.ToArray();
        }

        public IEnumerable<string> GetAssemblyNames(string workingDirectory)
        {
            var packages = GetPackages(workingDirectory).ToList();
            if (!packages.Any())
            {
                return Enumerable.Empty<string>();
            }

            var packagesFile = Path.Combine(workingDirectory, _fileSystem.PackagesFile);
            var packagesFolder = Path.Combine(workingDirectory, _fileSystem.PackagesFolder);

            var names = new List<string>();
            GetAssemblyNames(packagesFolder, packages, names, _fileSystem.FileExists(packagesFile));
            return names;
        }

        private void GetAssemblyNames(
            string packageDir,
            IEnumerable<IPackageReference> packageReferences,
            ICollection<string> names,
            bool strictLoad)
        {
            foreach (var packageReference in packageReferences)
            {
                var packageObject = _packageContainer.FindPackage(packageDir, packageReference);
                if (packageObject == null)
                {
                    _logger.WarnFormat(
                        "Cannot find: {0} {1}",
                        packageReference.PackageId,
                        packageReference.Version);

                    continue;
                }

                var compatibleDlls = packageObject.GetCompatibleDlls(packageReference.FrameworkName);
                if (compatibleDlls == null)
                {
                    _logger.WarnFormat(
                        "Cannot find compatible binaries for {0} in: {1} {2}",
                        packageReference.FrameworkName,
                        packageReference.PackageId,
                        packageReference.Version);

                    continue;
                }

                foreach (var name in compatibleDlls
                    .Select(packageFile => Path.Combine(packageDir, packageObject.FullName, packageFile))
                    .Where(path => _assemblyUtility.IsManagedAssembly(path))
                    .Concat(packageObject.FrameworkAssemblies)
                    .Where(name => !names.Contains(name)))
                {
                    names.Add(name);
                    _logger.Debug("Found: " + name);
                }

                if (packageObject.Dependencies == null || !packageObject.Dependencies.Any() || !strictLoad)
                {
                    continue;
                }

                var dependencyReferences = packageObject.Dependencies
                    .Where(dependency =>
                        _topLevelPackages.All(topLevelPackage => topLevelPackage.PackageId != dependency.Id))
                    .Select(dependency =>
                        new PackageReference(dependency.Id, dependency.FrameworkName, dependency.Version));

                GetAssemblyNames(packageDir, dependencyReferences, names, true);
            }
        }
    }
}
