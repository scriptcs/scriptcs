using System.ComponentModel.Composition.Hosting;
using ScriptCs.Package;

namespace ScriptCs.Command
{
    public class CommandFactory
    {
        private readonly ExportProvider _exportProvider;

        public CommandFactory(ExportProvider exportProvider)
        {
            _exportProvider = exportProvider;
        }

        public ICommand CreateCommand(ScriptCsArgs args)
        {
            if (args.ScriptName != null)
            {
                var fileSystem = _exportProvider.GetExportedValue<IFileSystem>();
                var resolver = _exportProvider.GetExportedValue<IPackageAssemblyResolver>();
                var scriptExecutor = _exportProvider.GetExportedValue<IScriptExecutor>();
                var scriptPackManager = new ScriptPackResolver(_exportProvider);
                return new ScriptExecuteCommand(args.ScriptName, fileSystem, resolver, scriptExecutor, scriptPackManager);
            }

            if (args.Install != null)
            {
                var fileSystem = _exportProvider.GetExportedValue<IFileSystem>();
                var resolver = _exportProvider.GetExportedValue<IPackageAssemblyResolver>();
                var packageInstaller = _exportProvider.GetExportedValue<IPackageInstaller>();

                return new InstallCommand(args.Install, args.AllowPreReleaseFlag, fileSystem, resolver, packageInstaller);
            }

            return new InvalidCommand();
        }
    }
}