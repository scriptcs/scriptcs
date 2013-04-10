using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using NuGet;

namespace ScriptCs.Package
{
    public class PackageContainer : IPackageContainer
    {
        private const string DotNetFramework = ".NETFramework";

        private readonly IFileSystem _fileSystem;

        public PackageContainer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> CreatePackageFile()
        {
            var packagesFile = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFile);
            var packageReferenceFile = new PackageReferenceFile(packagesFile);

            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFolder);
            var repository = new LocalPackageRepository(packagesFolder);

            var newestPackages = repository.GetPackages().GroupBy(p => p.Id)
                .Select(g => g.OrderByDescending(p => p.Version).FirstOrDefault());

            foreach (var package in newestPackages)
            {
                var newestFramework = GetNewestSupportedFramework(package);
                packageReferenceFile.AddEntry(package.Id, package.Version, newestFramework);

                yield return string.Format("{0}, Version {1}, .NET {2}", package.Id, package.Version, newestFramework.Version);
            }
        }

        public IPackageObject FindPackage(string path, IPackageReference packageRef)
        {
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
                return references.Select(i => new PackageReference(i.Id, i.TargetFramework, i.Version.Version, i.Version.SpecialVersion));
            }

            // No packages.config, check packages folder
            var packagesFolder = Path.Combine(_fileSystem.GetWorkingDirectory(path), Constants.PackagesFolder);
            if (_fileSystem.DirectoryExists(packagesFolder))
            {
                var repository = new LocalPackageRepository(packagesFolder);

                var arbitraryPackages = repository.GetPackages();
                if (arbitraryPackages.Any())
                {
                    return arbitraryPackages.Select(i => 
                        new PackageReference(i.Id, GetNewestSupportedFramework(i), i.Version.Version, i.Version.SpecialVersion));
                }
            }

            return Enumerable.Empty<IPackageReference>();
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