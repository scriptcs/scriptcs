using System;
using System.Runtime.Versioning;

namespace ScriptCs.Package
{
    public class PackageReference : IPackageReference
    {
        public PackageReference(string packageId, FrameworkName frameworkName, Version version)
        {
            FrameworkName = frameworkName;
            PackageId = packageId;
            Version = version;
        }

        public string PackageId { get; private set; }
        public FrameworkName FrameworkName { get; private set; }
        public Version Version { get; set; }
        public string SpecialVersion { get; set; }
    }
}