using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using Common.Logging;

using NuGet;

using ScriptCs.Contracts;

using IFileSystem = ScriptCs.Contracts.IFileSystem;
using PackageReference = ScriptCs.Package.PackageReference;

namespace ScriptCs.Hosting.Package
{
    public class PackageContainer : IPackageContainer
    {
        private const string DotNetFramework = ".NETFramework";

        private readonly IFileSystem _fileSystem;

        private readonly ILog _logger;

        public PackageContainer(IFileSystem fileSystem, ILog logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public void CreatePackageFile()
        {
            var packagesFile = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFile);
            var packageReferenceFile = new PackageReferenceFile(packagesFile);

            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFolder);
            var repository = new LocalPackageRepository(packagesFolder);

            var newestPackages = repository.GetPackages().GroupBy(p => p.Id)
                .Select(g => g.OrderByDescending(p => p.Version).FirstOrDefault());

            if (!newestPackages.Any())
            {
                _logger.Info("No packages found!");
                return;
            }

            _logger.InfoFormat("{0} {1}...", (File.Exists(packagesFile) ? "Updating" : "Creating") , Constants.PackagesFile);

            foreach (var package in newestPackages)
            {
                var newestFramework = GetNewestSupportedFramework(package);

                if (!packageReferenceFile.EntryExists(package.Id, package.Version))
                {
                    packageReferenceFile.AddEntry(package.Id, package.Version, newestFramework);

                    if (newestFramework == null)
                    {
                        _logger.InfoFormat("Added {0} (v{1}) to {2}", package.Id, package.Version, Constants.PackagesFile);
                    }
                    else
                    {
                        _logger.InfoFormat("Added {0} (v{1}, .NET {2}) to {3}", package.Id, package.Version, newestFramework.Version, Constants.PackagesFile);
                    }
  
                    continue;
                }

                _logger.InfoFormat("Skipped {0} because it already exists.", package.Id);
            }

            _logger.InfoFormat("Successfully {0} {1}...", (File.Exists(packagesFile) ? "updated" : "created"), Constants.PackagesFile);
        }

        public IPackageObject FindPackage(string path, IPackageReference packageRef)
        {
            Guard.AgainstNullArgument("packageRef", packageRef);

            var repository = new LocalPackageRepository(path);

            var package = packageRef.Version != null 
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
            var packagesFolder = Path.Combine(_fileSystem.GetWorkingDirectory(path), Constants.PackagesFolder);
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
                .Where(x => x.Identifier == DotNetFramework)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
        }
    }
}