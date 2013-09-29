using System;
using System.Runtime.Versioning;

using ScriptCs.Contracts;

namespace ScriptCs.Package
{
    public class PackageReference : IPackageReference
    {
        public PackageReference(string packageId, FrameworkName frameworkName, Version version)
            : this(packageId, frameworkName, version, null)
        {
        }

        public PackageReference(string packageId, FrameworkName frameworkName, Version version, string specialVersion)
        {
            FrameworkName = frameworkName;
            PackageId = packageId;
            Version = version;
            SpecialVersion = specialVersion;
        }

        public string PackageId { get; private set; }

        public FrameworkName FrameworkName { get; private set; }

        public Version Version { get; set; }

        public string SpecialVersion { get; set; }
    }
}