using System.IO;

namespace ScriptCs.Command
{
    public class CommandFactory
    {
        private readonly ScriptServices _scriptServices;

        public CommandFactory(ScriptServices scriptServices)
        {
            _scriptServices = scriptServices;
        }

        public ICommand CreateCommand(ScriptCsArgs args, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("args", args);

            if (args.Help)
            {
                return new ShowUsageCommand(_scriptServices.Logger, isValid: true);
            }

            if (args.Repl)
            {
                var replCommand = new ExecuteReplCommand(
                    _scriptServices.FileSystem,
                    _scriptServices.ScriptPackResolver,
                    _scriptServices.Engine,
                    _scriptServices.FilePreProcessor,
                    _scriptServices.Logger,
                    _scriptServices.Console,
                    _scriptServices.AssemblyResolver);

                return replCommand;
            }

            if (args.ScriptName != null)
            {
                var executeCommand = new ExecuteScriptCommand(
                    args.ScriptName,
                    scriptArgs,
                    _scriptServices.FileSystem,
                    _scriptServices.Executor,
                    _scriptServices.ScriptPackResolver,
                    _scriptServices.Logger,
                    _scriptServices.AssemblyResolver);

                var fileSystem = _scriptServices.FileSystem;
                var currentDirectory = fileSystem.CurrentDirectory;
                var packageFile = Path.Combine(currentDirectory, Constants.PackagesFile);
                var packagesFolder = Path.Combine(currentDirectory, Constants.PackagesFolder);

                if (fileSystem.FileExists(packageFile) && !fileSystem.DirectoryExists(packagesFolder))
                {
                    var installCommand = new InstallCommand(
                        null,
                        false,
                        fileSystem,
                        _scriptServices.PackageAssemblyResolver,
                        _scriptServices.PackageInstaller,
                        _scriptServices.Logger);

                    return new CompositeCommand(installCommand, executeCommand);
                }

                return executeCommand;
            }

            if (args.Install != null)
            {
                var installCommand = new InstallCommand(
                    args.Install,
                    args.AllowPreRelease,
                    _scriptServices.FileSystem,
                    _scriptServices.PackageAssemblyResolver,
                    _scriptServices.PackageInstaller,
                    _scriptServices.Logger
                    );

                string currentDirectory = null;

                if (args.Global)
                {
                    currentDirectory = Path.Combine(_scriptServices.FileSystem.LocalApplicationData, "scriptcs");
                    _scriptServices.FileSystem.CurrentDirectory = currentDirectory;
                    if (!_scriptServices.FileSystem.DirectoryExists(currentDirectory))
                        _scriptServices.FileSystem.CreateDirectory(currentDirectory);
                }
                else
                    currentDirectory = _scriptServices.FileSystem.CurrentDirectory;

                _scriptServices.InstallationProvider.Initialize();
                var packageFile = Path.Combine(currentDirectory, Constants.PackagesFile);

                if (!_scriptServices.FileSystem.FileExists(packageFile))
                {
                    var saveCommand = new SaveCommand(_scriptServices.PackageAssemblyResolver);
                    return new CompositeCommand(installCommand, saveCommand);
                }

                return installCommand;
            }

            if (args.Clean)
            {
                var saveCommand = new SaveCommand(_scriptServices.PackageAssemblyResolver);

                if (args.Global)
                {
                    var currentDirectory = Path.Combine(_scriptServices.FileSystem.LocalApplicationData, "scriptcs");
                    _scriptServices.FileSystem.CurrentDirectory = currentDirectory;
                    if (!_scriptServices.FileSystem.DirectoryExists(currentDirectory))
                        _scriptServices.FileSystem.CreateDirectory(currentDirectory);
                }

                var cleanCommand = new CleanCommand(
                    args.ScriptName,
                    _scriptServices.FileSystem,
                    _scriptServices.Logger);

                return new CompositeCommand(saveCommand, cleanCommand);
            }

            if (args.Save)
            {
                return new SaveCommand(_scriptServices.PackageAssemblyResolver);
            }

            if (args.Version)
            {
                return new VersionCommand(_scriptServices.Console);
            }

            return new ShowUsageCommand(_scriptServices.Logger, isValid: false);
        }
    }
}
