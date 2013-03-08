using System.Runtime.Versioning;

namespace ScriptCs.Package
{
    public class PackageReference : IPackageReference
    {
        public PackageReference(string packageId, FrameworkName frameworkName)
        {
            FrameworkName = frameworkName;
            PackageId = packageId;
        }

        public string PackageId { get; private set; }
        public FrameworkName FrameworkName { get; private set; }
    }
}