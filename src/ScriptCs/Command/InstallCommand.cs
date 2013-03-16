using System;
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

        public InstallCommand(string name, bool allowPre, IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver, IPackageInstaller packageInstaller)
        {
            _name = name;
            _allowPre = allowPre;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _packageInstaller = packageInstaller;
        }

        public void Execute()
        {
            var workingDirectory = _fileSystem.CurrentDirectory;
            IEnumerable<IPackageReference> pkgs;

            if (string.IsNullOrWhiteSpace(_name))
            {
                pkgs = _packageAssemblyResolver.GetPackages(workingDirectory);
            }
            else
            {
                pkgs = new[] { new PackageReference(_name, new FrameworkName(".NETFramework,Version=v4.0"), new Version()) };
            }
            _packageInstaller.InstallPackages(pkgs, _allowPre, msg => Console.WriteLine(msg));
        }
    }
}