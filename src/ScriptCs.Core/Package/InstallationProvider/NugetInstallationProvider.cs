using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;

namespace ScriptCs.Package.InstallationProvider
{
    internal class NugetInstallationProvider : IInstallationProvider
    {
        private readonly PackageManager _manager;
        private readonly IEnumerable<string> _repositoryUrls;

        public NugetInstallationProvider(IFileSystem fileSystem)
        {
            var path = Path.Combine(fileSystem.CurrentDirectory, Constants.PackagesFolder);
            _repositoryUrls = GetRepositorySources(path);
            var remoteRepository = new AggregateRepository(PackageRepositoryFactory.Default, _repositoryUrls, true);
            _manager = new PackageManager(remoteRepository, path);
        }

        public IEnumerable<string> GetRepositorySources(string path)
        {
            var configFileSystem = new PhysicalFileSystem(path);
            var settings = Settings.LoadDefaultSettings(configFileSystem);
            if (settings == null) return new[] { Constants.DefaultRepositoryUrl };

            var sourceProvider = new PackageSourceProvider(settings);
            var sources = sourceProvider.LoadPackageSources();

            if (sources == null || !sources.Any()) return new[] { Constants.DefaultRepositoryUrl };

            return sources.Select(i => i.Source);
        }

        public bool InstallPackage(IPackageReference packageId, bool allowPreRelease = false, Action<string> packageInstalled = null)
        {
            var useVersion = packageId.Version.CompareTo(new Version()) != 0;
            try
            {
                if (useVersion)
                {
                    _manager.InstallPackage(packageId.PackageId,
                                            new SemanticVersion(packageId.Version, packageId.SpecialVersion), false,
                                            allowPreRelease);
                }
                else
                {
                    _manager.InstallPackage(packageId.PackageId);
                }

                if (packageInstalled != null)
                    packageInstalled("Installed: " + packageId.PackageId + " " + (useVersion ? packageId.Version.ToString() : ""));

                return true;
            }
            catch (Exception e)
            {
                if (packageInstalled != null)
                {
                    packageInstalled("Installation failed: " + packageId.PackageId + " " + (useVersion ? packageId.Version.ToString() : ""));
                    packageInstalled(e.Message);
                }
                return false;
            }
        }
    }
}