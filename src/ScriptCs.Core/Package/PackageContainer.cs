using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace ScriptCs.Package
{
    public class PackageContainer : IPackageContainer
    {
        public IPackageObject FindPackage(string path, string packageId)
        {
            var repository = new LocalPackageRepository(path);

            var package = repository.FindPackage(packageId);
            return package == null ? null : new PackageObject(package);
        }

        public IEnumerable<IPackageReference> FindReferences(string path)
        {
            var packageReferenceFile = new PackageReferenceFile(path);
            var references = packageReferenceFile.GetPackageReferences();
            if (references == null) return Enumerable.Empty<IPackageReference>();

            var packages = references.Select(i => new PackageReference(i.Id, i.TargetFramework));
            return packages;
        }
    }
}