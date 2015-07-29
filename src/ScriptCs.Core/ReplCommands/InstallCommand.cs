using System;
using System.Linq;
using System.Runtime.Versioning;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class InstallCommand : IReplCommand
    {
        private readonly IPackageInstaller _packageInstaller;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly ILog _logger;
        private readonly IInstallationProvider _installationProvider;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public InstallCommand(
            IPackageInstaller packageInstaller,
            IPackageAssemblyResolver packageAssemblyResolver,
            Common.Logging.ILog logger,
            IInstallationProvider installationProvider)
            :this(packageInstaller, packageAssemblyResolver,new CommonLoggingLogProvider(logger), installationProvider)
        {
        }

        public InstallCommand(
            IPackageInstaller packageInstaller,
            IPackageAssemblyResolver packageAssemblyResolver,
            ILogProvider logProvider,
            IInstallationProvider installationProvider)
        {
            Guard.AgainstNullArgument("packageInstaller", packageInstaller);
            Guard.AgainstNullArgument("packageAssemblyResolver", packageAssemblyResolver);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("installationProvider", installationProvider);

            _packageInstaller = packageInstaller;
            _packageAssemblyResolver = packageAssemblyResolver;
            _logger = logProvider.ForCurrentType();
            _installationProvider = installationProvider;
        }

        public string Description
        {
            get { return "Installs a Nuget package. I.e. :install <package> <version>"; }
        }

        public string CommandName
        {
            get { return "install"; }
        }

        public object Execute(IRepl repl, object[] args)
        {
            Guard.AgainstNullArgument("repl", repl);

            if (args == null || args.Length == 0)
            {
                return null;
            }

            string version = null;
            if (args.Length >= 2)
            {
                version = args[1].ToString();
            }

            var allowPre = args.Length >= 3 && args[2].ToString().ToUpperInvariant() == "PRE";

            _logger.InfoFormat("Installing {0}", args[0]);

            _installationProvider.Initialize();

            var packageRef = new PackageReference(
                args[0].ToString(), new FrameworkName(".NETFramework,Version=v4.0"), version);

            _packageInstaller.InstallPackages(new[] { packageRef }, allowPre);
            _packageAssemblyResolver.SavePackages();

            var dlls = _packageAssemblyResolver.GetAssemblyNames(repl.FileSystem.CurrentDirectory)
                .Except(repl.References.Paths).ToArray();

            repl.AddReferences(dlls);

            foreach (var dll in dlls)
            {
                _logger.InfoFormat("Added reference to {0}", dll);
            }

            return null;
        }
    }
}
