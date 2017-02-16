using System.Runtime.Versioning;
using NuGet;

namespace ScriptCs.Hosting.Package
{
    //needed to allow forcing framework version for installation as the InstallPackage method that accepts version is protected and all other overloads force version to null!
    public class ScriptCsPackageManager : PackageManager
    {
        public ScriptCsPackageManager(IPackageRepository sourceRepository, string path) : base(sourceRepository, path)
        {
        }

        public override void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            base.InstallPackage(package, new FrameworkName(".NETFramework,Version=v4.5"),  ignoreDependencies, allowPrereleaseVersions);
        }

        public override void InstallPackage(string packageId, SemanticVersion version, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            var package = PackageRepositoryHelper.ResolvePackage(SourceRepository, LocalRepository, packageId, version, allowPrereleaseVersions);
            base.InstallPackage(package, new FrameworkName(".NETFramework,Version=v4.5"), ignoreDependencies, allowPrereleaseVersions);
        }
    }
}