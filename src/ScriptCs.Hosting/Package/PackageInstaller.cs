using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Package
{
    public class PackageInstaller : IPackageInstaller
    {
        private readonly IInstallationProvider _installer;
        private readonly ILog _logger;

        public PackageInstaller(IInstallationProvider installer, ILog logger)
        {
            _installer = installer;
            _logger = logger;
        }

        public void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false)
        {
            if (packageIds == null)
            {
                throw new ArgumentNullException("packageIds");
            }

            packageIds = packageIds.ToList();

            if (!packageIds.Any())
            {
                _logger.Info("Nothing to install.");
                return;
            }

            var exceptions = new List<Exception>();
            foreach (var packageId in packageIds)
            {
                if (_installer.IsInstalled(packageId, allowPreRelease))
                {
                    continue;
                }

                try
                {
                    _installer.InstallPackage(packageId, allowPreRelease);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
