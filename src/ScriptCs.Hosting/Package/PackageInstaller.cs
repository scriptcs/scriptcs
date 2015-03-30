using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;
using ScriptCs.Contracts.Logging;

namespace ScriptCs.Hosting.Package
{
    public class PackageInstaller : IPackageInstaller
    {
        private readonly IInstallationProvider _installer;
        private readonly ILog _logger;

        public PackageInstaller(IInstallationProvider installer, ILog logger)
        {
            Guard.AgainstNullArgument("installer", installer);
            Guard.AgainstNullArgument("logger", logger);

            _installer = installer;
            _logger = logger;
        }

        public void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false)
        {
            Guard.AgainstNullArgument("packageIds", packageIds);

            packageIds = packageIds.Where(packageId => !_installer.IsInstalled(packageId, allowPreRelease)).ToList();

            if (!packageIds.Any())
            {
                _logger.Info("Nothing to install.");
                return;
            }

            var exceptions = new List<Exception>();
            foreach (var packageId in packageIds)
            {
                try
                {
                    _installer.InstallPackage(packageId, allowPreRelease);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error installing package.", ex);
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
