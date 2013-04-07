using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;

namespace ScriptCs.Package
{
    public class PackageContainer : IPackageContainer
    {
        private readonly IFileSystem _fileSystem;

        public PackageContainer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> CreatePackageFile()
        {
            var packageReferenceFile = new PackageReferenceFile(Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFile));
            var repository = new LocalPackageRepository(Path.Combine(_fileSystem.CurrentDirectory, Constants.PackagesFolder));
            var arbitraryPackages = repository.GetPackages();
            var result = new List<string>();

            foreach (var package in arbitraryPackages)
            {
                packageReferenceFile.AddEntry(package.Id, package.Version, VersionUtility.ParseFrameworkName("net45"));
                result.Add(string.Format("{0}, Version {1}, .NET 4.5", package.Id, package.Version));
            }

            return result;
        }

        public IPackageObject FindPackage(string path, IPackageReference packageRef)
        {
            var repository = new LocalPackageRepository(path);
            IPackage package;
            if (packageRef.Version != null)
            {
                package = repository.FindPackage(packageRef.PackageId, new SemanticVersion(packageRef.Version, packageRef.SpecialVersion), true, true);
            }
            else
            {
                package = repository.FindPackage(packageRef.PackageId);
            }

            return package == null ? null : new PackageObject(package, packageRef.FrameworkName);
        }

        public IEnumerable<IPackageReference> FindReferences(string path)
        {
            var packageReferenceFile = new PackageReferenceFile(path);
            var references = packageReferenceFile.GetPackageReferences();

            if (!references.Any())
            {
                var packagesFolder = Path.Combine(_fileSystem.GetWorkingDirectory(path), Constants.PackagesFolder);
                if (_fileSystem.DirectoryExists(packagesFolder))
                {
                    var repository = new LocalPackageRepository(packagesFolder);
                    var arbitraryPackages = repository.GetPackages().Where(i => i.GetSupportedFrameworks().Any(x => x.FullName == VersionUtility.ParseFrameworkName("net40").FullName)).ToList();
                    if (arbitraryPackages.Any())
                    {
                        return arbitraryPackages.Select(i => new PackageReference(i.Id, VersionUtility.ParseFrameworkName("net40"), i.Version.Version) { SpecialVersion = i.Version.SpecialVersion });
                    }
                }

                return Enumerable.Empty<IPackageReference>();
            }

            var packages = references.Select(i => new PackageReference(i.Id, i.TargetFramework, i.Version.Version) { SpecialVersion = i.Version.SpecialVersion });
            return packages;
        }
    }
}