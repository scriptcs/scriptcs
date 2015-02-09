using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Package
{
    public class PackageInstaller : IPackageInstaller
    {
        private readonly IInstallationProvider _installer;
        private readonly ILog _logger;
        private readonly IPackageScriptsComposer _scriptsComposer;

        public PackageInstaller(IInstallationProvider installer, ILog logger, IPackageScriptsComposer scriptsComposer)
        {
            Guard.AgainstNullArgument("installer", installer);
            Guard.AgainstNullArgument("logger", logger);
            Guard.AgainstNullArgument("scriptsComposer", scriptsComposer);

            _installer = installer;
            _logger = logger;
            _scriptsComposer = scriptsComposer;
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
                    _logger.Error(ex.Message, ex);
                    exceptions.Add(ex);
                }
            }
            var builder = new StringBuilder();
            _scriptsComposer.Compose(packageIds, builder);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
