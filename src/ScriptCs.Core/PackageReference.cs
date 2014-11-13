using System;
using System.Runtime.Versioning;
using ScriptCs.Contracts;

namespace ScriptCs
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

        public PackageReference(string packageId, FrameworkName frameworkName, string stringVersion)
        {
            FrameworkName = frameworkName;
            PackageId = packageId;
            SetVersionFromString(stringVersion);
        }

        public string PackageId { get; private set; }

        public FrameworkName FrameworkName { get; private set; }

        public Version Version { get; set; }

        public string SpecialVersion { get; set; }

        private void SetVersionFromString(string stringVersion)
        {
            if (string.IsNullOrWhiteSpace(stringVersion))
            {
                Version = new Version();
            }
            else
            {
                if (stringVersion.Contains("-"))
                {
                    var splitVersion = stringVersion.Split(new[] { '-' }, 2);
                    if (splitVersion.Length == 2)
                    {
                        Version = new Version(splitVersion[0]);
                        SpecialVersion = splitVersion[1];
                        return;
                    }
                }

                Version = new Version(stringVersion);
            }
        }
    }
}