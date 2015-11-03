using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class InstallCommand : IInstallCommand
    {
        private readonly string _name;
        private readonly string _version;
        private readonly bool _allowPre;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IPackageInstaller _packageInstaller;
        private readonly IScriptLibraryComposer _composer;
        private readonly ILog _logger;

        public InstallCommand(
            string name,
            string version,
            bool allowPre,
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IPackageInstaller packageInstaller,
            IScriptLibraryComposer composer,
            ILogProvider logger)
        {
            _name = name;
            _version = version ?? string.Empty;
            _allowPre = allowPre;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _packageInstaller = packageInstaller;
            _composer = composer;
            _logger = logger.ForCurrentType();
        }

        public CommandResult Execute()
        {
            _logger.Info("Installing packages...");

            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            if (!string.IsNullOrWhiteSpace(_composer.ScriptLibrariesFile))
            {
                var scriptLibrariesFile = Path.Combine(packagesFolder, _composer.ScriptLibrariesFile);

                if (_fileSystem.FileExists(scriptLibrariesFile))
                {
                    _logger.DebugFormat("Deleting: {0}", scriptLibrariesFile);
                    _fileSystem.FileDelete(scriptLibrariesFile);
                }
            }

            var packages = GetPackages(_fileSystem.CurrentDirectory);
            try
            {
                _packageInstaller.InstallPackages(packages, _allowPre);
                _logger.Info("Package installation succeeded.");
                return CommandResult.Success;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Package installation failed.", ex);
                return CommandResult.Error;
            }
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

            yield return new PackageReference(_name, new FrameworkName(".NETFramework,Version=v4.0"), _version);
        }
    }
}