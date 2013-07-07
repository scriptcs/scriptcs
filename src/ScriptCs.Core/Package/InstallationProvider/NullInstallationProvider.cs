using System;
using System.Collections.Generic;

namespace ScriptCs.Package.InstallationProvider
{
    public class NullInstallationProvider : IInstallationProvider
    {
        public IEnumerable<string> GetRepositorySources(string path)
        {
            return new string[0];
        }

        public bool IsInstalled(IPackageReference packageId, bool allowPreRelease = false)
        {
            return false;
        }

        public bool InstallPackage(IPackageReference packageId, bool allowPreRelease = false, Action<string> packageInstalled = null)
        {
            return false;
        }
    }
}