using System.Linq;
using System.Runtime.Versioning;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.ReplCommands
{
    public class InstallCommand : IReplCommand
    {
        private readonly IPackageInstaller _packageInstaller;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly ILog _logger;

        public InstallCommand(IPackageInstaller packageInstaller, IPackageAssemblyResolver packageAssemblyResolver, ILog logger)
        {
            _packageInstaller = packageInstaller;
            _packageAssemblyResolver = packageAssemblyResolver;
            _logger = logger;
        }

        public string CommandName
        {
            get { return "install"; }
        }

        public object Execute(IScriptExecutor repl, object[] args)
        {
            if (args == null || args.Length == 0) return null;

            string version = null;
            var allowPre = false;
            if (args.Length >= 2)
            {
                version = args[1].ToString();
                if (args.Length == 3)
                {
                    allowPre = true;
                }
            }

            _logger.InfoFormat("Installing {0}", args[0]);

            var packageRef = new PackageReference(args[0].ToString(), new FrameworkName(".NETFramework,Version=v4.0"), version);
            _packageInstaller.InstallPackages(new[]
            {
                packageRef
            }, allowPre);
            _packageAssemblyResolver.SavePackages();

            var dlls = _packageAssemblyResolver.GetAssemblyNames(repl.FileSystem.CurrentDirectory).Except(repl.References.PathReferences).ToArray();
            repl.AddReferences(dlls);

            foreach (var dll in dlls)
            {
                _logger.InfoFormat("Added reference to {0}", dll);
            }

            return null;
        }
    }
}