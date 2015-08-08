using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;
using ScriptCs.Contracts;
using IFileSystem = ScriptCs.Contracts.IFileSystem;

namespace ScriptCs.Hosting.Package
{
    public class PackageContainer : IPackageContainer
    {
        private const string DotNetFramework = ".NETFramework";

        private const string DotNetPortable = ".NETPortable";

        private readonly IFileSystem _fileSystem;

        private readonly ILog _logger;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public PackageContainer(IFileSystem fileSystem, Common.Logging.ILog logger)
            : this(fileSystem, new CommonLoggingLogProvider(logger))
        {
        }

        public PackageContainer(IFileSystem fileSystem, ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logProvider", logProvider);

            _fileSystem = fileSystem;
            _logger = logProvider.ForCurrentType();
        }

        public void CreatePackageFile()
        {
            var packagesFile = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFile);
            var packageReferenceFile = new PackageReferenceFile(packagesFile);

            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            var repository = new LocalPackageRepository(packagesFolder);

            var newestPackages = repository.GetPackages().GroupBy(p => p.Id)
                .Select(g => g.OrderByDescending(p => p.Version).FirstOrDefault());

            if (!newestPackages.Any())
            {
                _logger.Info("No packages found!");
                return;
            }

            _logger.InfoFormat("{0} {1}...", (File.Exists(packagesFile) ? "Updating" : "Creating"), _fileSystem.PackagesFile);

            foreach (var package in newestPackages)
            {
                var newestFramework = GetNewestSupportedFramework(package);

                if (!packageReferenceFile.EntryExists(package.Id, package.Version))
                {
                    packageReferenceFile.AddEntry(package.Id, package.Version, package.DevelopmentDependency, newestFramework);

                    if (newestFramework == null)
                    {
                        _logger.InfoFormat("Added {0} (v{1}) to {2}", package.Id, package.Version, _fileSystem.PackagesFile);
                    }
                    else
                    {
                        _logger.InfoFormat("Added {0} (v{1}, .NET {2}) to {3}", package.Id, package.Version, newestFramework.Version, _fileSystem.PackagesFile);
                    }

                    continue;
                }

                _logger.InfoFormat("Skipped {0} because it already exists.", package.Id);
            }

            _logger.InfoFormat("Successfully {0} {1}.", (File.Exists(packagesFile) ? "updated" : "created"), _fileSystem.PackagesFile);
        }

        public IPackageObject FindPackage(string path, IPackageReference packageRef)
        {
            Guard.AgainstNullArgument("packageRef", packageRef);

            var repository = new LocalPackageRepository(path);

            var package = packageRef.Version != null && !(packageRef.Version.Major == 0 && packageRef.Version.Minor == 0)
                ? repository.FindPackage(packageRef.PackageId, new SemanticVersion(packageRef.Version, packageRef.SpecialVersion), true, true)
                : repository.FindPackage(packageRef.PackageId);

            return package == null ? null : new PackageObject(package, packageRef.FrameworkName);
        }

        public IEnumerable<IPackageReference> FindReferences(string path)
        {
            var packageReferenceFile = new PackageReferenceFile(path);

            var references = packageReferenceFile.GetPackageReferences().ToList();
            if (references.Any())
            {
                foreach (var packageReference in references)
                {
                    yield return new PackageReference(
                            packageReference.Id,
                            packageReference.TargetFramework,
                            packageReference.Version.Version,
                            packageReference.Version.SpecialVersion);
                }

                yield break;
            }

            // No packages.config, check packages folder
            var packagesFolder = Path.Combine(_fileSystem.GetWorkingDirectory(path), _fileSystem.PackagesFolder);
            if (!_fileSystem.DirectoryExists(packagesFolder))
            {
                yield break;
            }

            var repository = new LocalPackageRepository(packagesFolder);

            var arbitraryPackages = repository.GetPackages();
            if (!arbitraryPackages.Any())
            {
                yield break;
            }

            foreach (var arbitraryPackage in arbitraryPackages)
            {
                var newestFramework = GetNewestSupportedFramework(arbitraryPackage)
                    ?? VersionUtility.EmptyFramework;

                yield return new PackageReference(
                        arbitraryPackage.Id,
                        newestFramework,
                        arbitraryPackage.Version.Version,
                        arbitraryPackage.Version.SpecialVersion);
            }
        }

        private static FrameworkName GetNewestSupportedFramework(IPackage packageMetadata)
        {
            return packageMetadata.GetSupportedFrameworks()
                .Where(IsValidFramework)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
        }

        private static bool IsValidFramework(FrameworkName frameworkName)
        {
            return frameworkName.Identifier == DotNetFramework
                || (frameworkName.Identifier == DotNetPortable
                    && frameworkName.Profile.Split('+').Any(IsValidProfile));
        }

        private static bool IsValidProfile(string profile)
        {
            return profile == "net40" || profile == "net45";
        }
    }
}