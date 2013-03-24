﻿using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs.Package
{
    public class PackageInstaller : IPackageInstaller
    {
        private readonly IInstallationProvider _installer;

        public PackageInstaller(IInstallationProvider installer)
        {
            _installer = installer;
        }

        public void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false, Action<string> packageInstalled = null)
        {
            if (packageIds == null) throw new ArgumentNullException("packageIds");
            packageIds = packageIds.ToList();

            if (!packageIds.Any())
            {
                if (packageInstalled != null)
                {
                    packageInstalled("Nothing to install.");
                }

                return;
            }

            var successful = packageIds.Select(packageId => _installer.InstallPackage(packageId, allowPreRelease, packageInstalled))
                .Aggregate(true, (current, result) => current && result);

            if (packageInstalled != null && packageIds.Count() > 1)
            {
                packageInstalled(successful ? "Installation successful." : "Installation unsuccessful.");
            }
        }
    }
}