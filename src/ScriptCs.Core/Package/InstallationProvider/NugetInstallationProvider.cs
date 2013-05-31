using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;

namespace ScriptCs.Package.InstallationProvider
{
    public class NugetInstallationProvider : IInstallationProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly PackageManager _manager;
        private readonly IEnumerable<string> _repositoryUrls;

        private static readonly Version EmptyVersion = new Version();

        public NugetInstallationProvider(IFileSystem fileSystem)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);

            _fileSystem = fileSystem;
            var path = Path.Combine(fileSystem.CurrentDirectory, Constants.PackagesFolder);
            _repositoryUrls = GetRepositorySources(path);
            var remoteRepository = new AggregateRepository(PackageRepositoryFactory.Default, _repositoryUrls, true);
            _manager = new PackageManager(remoteRepository, path);
        }

        public IEnumerable<string> GetRepositorySources(string path)
        {
            var configFileSystem = new PhysicalFileSystem(path);

            ISettings settings;
            if (_fileSystem.FileExists(Path.Combine(_fileSystem.CurrentDirectory, Constants.NugetFile)))
            {
                settings = new Settings(configFileSystem, Constants.NugetFile);
            }
            else
            {
                settings = Settings.LoadDefaultSettings(configFileSystem);
            }

            if (settings == null) return new[] { Constants.DefaultRepositoryUrl };

            var sourceProvider = new PackageSourceProvider(settings);
            var sources = sourceProvider.LoadPackageSources();

            if (sources == null || !sources.Any()) return new[] { Constants.DefaultRepositoryUrl };

            return sources.Select(i => i.Source);
        }

        public bool InstallPackage(IPackageReference packageId, bool allowPreRelease = false, Action<string> packageInstalled = null)
        {
            Guard.AgainstNullArgument("packageId", packageId);

            var version = GetVersion(packageId);
            var packageName = packageId.PackageId + " " + (version == null ? string.Empty : packageId.Version.ToString());
            try
            {
                _manager.InstallPackage(packageId.PackageId, version, allowPrereleaseVersions: allowPreRelease, ignoreDependencies: false);
                if(packageInstalled != null)
                {
                    packageInstalled("Installed: " + packageName);
                }
                return true;
            }
            catch (Exception e)
            {
                if (packageInstalled != null)
                {
                    packageInstalled("Installation failed: " + packageName);
                    packageInstalled(e.Message);
                }
                return false;
            }
        }

        private static SemanticVersion GetVersion(IPackageReference packageReference)
        {
            return packageReference.Version == EmptyVersion ? null : new SemanticVersion(packageReference.Version, packageReference.SpecialVersion);
        }

        public bool IsInstalled(IPackageReference packageReference, bool allowPreRelease = false)
        {
            Guard.AgainstNullArgument("packageReference", packageReference);

            var version = GetVersion(packageReference);
            return _manager.LocalRepository.FindPackage(packageReference.PackageId, version, allowPreRelease, allowUnlisted: false) != null;
        }
    }
}