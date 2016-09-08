using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;
using ScriptCs.Contracts;
using IFileSystem = ScriptCs.Contracts.IFileSystem;

namespace ScriptCs.Hosting.Package
{
    public class NugetInstallationProvider : IInstallationProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;
        private PackageManager _manager;
        private IEnumerable<string> _repositoryUrls;

        private static readonly Version EmptyVersion = new Version();

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public NugetInstallationProvider(IFileSystem fileSystem, Common.Logging.ILog logger)
            : this(fileSystem, new CommonLoggingLogProvider(logger))
        {
        }

        public NugetInstallationProvider(IFileSystem fileSystem, ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logProvider", logProvider);

            _fileSystem = fileSystem;
            _logger = logProvider.ForCurrentType();
        }

        public void Initialize()
        {
            var path = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            _repositoryUrls = GetRepositorySources(path);
            var remoteRepository = new AggregateRepository(PackageRepositoryFactory.Default, _repositoryUrls, true);
            _manager = new PackageManager(remoteRepository, path);
        }

        public IEnumerable<string> GetRepositorySources(string path)
        {
            var configFileSystem = new PhysicalFileSystem(path);

            ISettings settings;
            var localNuGetConfigFile = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.NugetFile);
            if (_fileSystem.FileExists(localNuGetConfigFile))
            {
                settings = Settings.LoadDefaultSettings(configFileSystem, localNuGetConfigFile, null);
            }
            else
            {
                settings = Settings.LoadDefaultSettings(configFileSystem, null, new NugetMachineWideSettings());
            }

            if (settings == null)
            {
                return new[] { Constants.DefaultRepositoryUrl };
            }

            var sourceProvider = new PackageSourceProvider(settings);

            HttpClient.DefaultCredentialProvider = new SettingsCredentialProvider(NullCredentialProvider.Instance, sourceProvider);

            var sources = sourceProvider.LoadPackageSources().Where(i => i.IsEnabled == true);

            if (sources == null || !sources.Any())
            {
                return new[] { Constants.DefaultRepositoryUrl };
            }

            return sources.Select(i => i.Source);
        }

        public void InstallPackage(IPackageReference packageId, bool allowPreRelease = false)
        {
            Guard.AgainstNullArgument("packageId", packageId);

            var version = GetVersion(packageId);
            var packageName = packageId.PackageId + " " + (version == null ? string.Empty : packageId.Version.ToString());
            _manager.InstallPackage(packageId.PackageId, version, allowPrereleaseVersions: allowPreRelease, ignoreDependencies: false);
            _logger.Info("Installed: " + packageName);
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