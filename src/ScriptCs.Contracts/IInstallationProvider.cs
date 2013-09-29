using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IInstallationProvider
    {
        IEnumerable<string> GetRepositorySources(string path);
        void Initialize();
        bool IsInstalled(IPackageReference packageId, bool allowPreRelease = false);
        bool InstallPackage(IPackageReference packageId, bool allowPreRelease = false);
    }
}