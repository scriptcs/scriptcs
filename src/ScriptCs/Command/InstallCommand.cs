using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Common.Logging;
using ScriptCs.Package;

using ServiceStack.Text;

namespace ScriptCs.Command
{
    internal class InstallCommand : IInstallCommand
    {
        private readonly string _name;

        private readonly bool _allowPre;

        private readonly IFileSystem _fileSystem;

        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly IPackageInstaller _packageInstaller;

        private readonly ILog _logger;

        public InstallCommand(
            string name,
            bool allowPre,
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IPackageInstaller packageInstaller,
            ILog logger)
        {
            _name = name;
            _allowPre = allowPre;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _packageInstaller = packageInstaller;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.Info("Installing packages...");

            var workingDirectory = _fileSystem.CurrentDirectory;
            var packages = GetPackages(workingDirectory);

            try
            {
                _packageInstaller.InstallPackages(packages, _allowPre, _logger.Info);

                UpdateManifest(workingDirectory);

                _logger.Info("Installation completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Installation failed: {0}.", e.Message);
                return CommandResult.Error;
            }
        }

        private void UpdateManifest(string workingDirectory)
        {
            var manifestPath = Path.Combine(workingDirectory, Constants.ManifestFile);
            var manifest = GetManifest(manifestPath) ?? new ScriptManifest();

            var installedPackages = _packageAssemblyResolver.GetAssemblyNames(workingDirectory);
            manifest.PackageAssemblies.UnionWith(installedPackages);

            _fileSystem.WriteToFile(manifestPath, manifest.ToJson());
        }

        private ScriptManifest GetManifest(string manifestPath)
        {
            if (!_fileSystem.FileExists(manifestPath)) return null;
            return _fileSystem.ReadFile(manifestPath).FromJson<ScriptManifest>();
        }

        private IEnumerable<IPackageReference> GetPackages(string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                var packages = _packageAssemblyResolver.GetPackages(workingDirectory);
                foreach (var packageReference in packages)
                {
                    yield return packageReference;
                }

                yield break;
            }

            yield return new PackageReference(_name, new FrameworkName(".NETFramework,Version=v4.0"), new Version());
        }
    }
}