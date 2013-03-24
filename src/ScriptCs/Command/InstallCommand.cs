﻿using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

using ScriptCs.Package;

namespace ScriptCs.Command
{
    internal class InstallCommand : IInstallCommand
    {
        private readonly string _name;

        private readonly bool _allowPre;

        private readonly IFileSystem _fileSystem;

        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly IPackageInstaller _packageInstaller;

        public InstallCommand(
            string name,
            bool allowPre,
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IPackageInstaller packageInstaller)
        {
            _name = name;
            _allowPre = allowPre;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _packageInstaller = packageInstaller;
        }

        public CommandResult Execute()
        {
            Console.WriteLine("Installing packages...");

            var workingDirectory = _fileSystem.CurrentDirectory;
            var packages = GetPackages(workingDirectory);

            try
            {
                _packageInstaller.InstallPackages(packages, _allowPre, Console.WriteLine);

                Console.WriteLine("Installation completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine("Installation failed: {0}.", e.Message);
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

            yield return new PackageReference(_name, new FrameworkName(".NETFramework,Version=v4.0"), new Version());
        }
    }
}