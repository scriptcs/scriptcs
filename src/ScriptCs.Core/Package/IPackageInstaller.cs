using System;
using System.Collections.Generic;

namespace ScriptCs.Package
{
    public interface IPackageInstaller
    {
        void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false, Action<string> packageInstalled = null);
    }
}