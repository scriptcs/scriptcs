using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IPackageInstaller
    {
        void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false);
    }
}