using System;
using System.Collections.Generic;

namespace ScriptCs.Package.InstallationProvider
{
    public interface IInstallationProvider
    {
        IEnumerable<string> GetRepositorySources(string path);
        bool InstallPackage(IPackageReference packageId, bool allowPreRelease = false, Action<string> packageInstalled = null);
    }
}