using System.IO;

namespace ScriptCs.Command
{
    public class CommandFactory
    {
        private readonly ScriptServiceRoot _scriptServiceRoot;

        public CommandFactory(ScriptServiceRoot scriptServiceRoot)
        {
            _scriptServiceRoot = scriptServiceRoot;
        }

        public ICommand CreateCommand(ScriptCsArgs args, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("args", args);

            if (args.Help)
            {
                return new ShowUsageCommand(_scriptServiceRoot.Logger, isValid: true);
            }

            if (args.Repl)
            {
                var replCommand = new ExecuteReplCommand(
                    _scriptServiceRoot.FileSystem, _scriptServiceRoot.ScriptPackResolver,
                    _scriptServiceRoot.Engine, _scriptServiceRoot.FilePreProcessor, _scriptServiceRoot.Logger, _scriptServiceRoot.Console,
                    _scriptServiceRoot.AssemblyName);
                return replCommand;
            }

            if (args.ScriptName != null)
            {
                var executeCommand = new ExecuteScriptCommand(
                    args.ScriptName,
                    scriptArgs,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.Executor,
                    _scriptServiceRoot.ScriptPackResolver,
                    _scriptServiceRoot.Logger,
                    _scriptServiceRoot.AssemblyName);

                var fileSystem = _scriptServiceRoot.FileSystem;
                var currentDirectory = fileSystem.CurrentDirectory;
                var packageFile = Path.Combine(currentDirectory, Constants.PackagesFile);
                var packagesFolder = Path.Combine(currentDirectory, Constants.PackagesFolder);

                if (fileSystem.FileExists(packageFile) && !fileSystem.DirectoryExists(packagesFolder))
                {
                    var installCommand = new InstallCommand(
                        null,
                        false,
                        fileSystem,
                        _scriptServiceRoot.PackageAssemblyResolver,
                        _scriptServiceRoot.PackageInstaller,
                        _scriptServiceRoot.Logger);

                    var restoreCommand = new RestoreCommand(
                        args.ScriptName,
                        _scriptServiceRoot.FileSystem,
                        _scriptServiceRoot.PackageAssemblyResolver,
                        _scriptServiceRoot.Logger);

                    return new CompositeCommand(installCommand, restoreCommand, executeCommand);
                }

                if (args.Restore)
                {
                    var restoreCommand = new RestoreCommand(
                        args.ScriptName, 
                        _scriptServiceRoot.FileSystem, 
                        _scriptServiceRoot.PackageAssemblyResolver,
                        _scriptServiceRoot.Logger);

                    return new CompositeCommand(restoreCommand, executeCommand);
                }

                return executeCommand;
            }

            if (args.Install != null)
            {
                var installCommand = new InstallCommand(
                    args.Install,
                    args.AllowPreRelease,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.PackageInstaller,
                    _scriptServiceRoot.Logger);

                var restoreCommand = new RestoreCommand(
                    args.Install,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.Logger);

                var currentDirectory = _scriptServiceRoot.FileSystem.CurrentDirectory;
                var packageFile = Path.Combine(currentDirectory, Constants.PackagesFile);

                if (!_scriptServiceRoot.FileSystem.FileExists(packageFile))
                {
                    var saveCommand = new SaveCommand(_scriptServiceRoot.PackageAssemblyResolver);
                    return new CompositeCommand(installCommand, restoreCommand, saveCommand);
                }

                return new CompositeCommand(installCommand, restoreCommand);
            }

            if (args.Clean)
            {
                var saveCommand = new SaveCommand(_scriptServiceRoot.PackageAssemblyResolver);

                var cleanCommand = new CleanCommand(
                    args.ScriptName,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.Logger);

                return new CompositeCommand(saveCommand, cleanCommand);
            }

            if (args.Save)
            {
                return new SaveCommand(_scriptServiceRoot.PackageAssemblyResolver);
            }

            if (args.Version)
            {
                return new VersionCommand();
            }

            return new ShowUsageCommand(_scriptServiceRoot.Logger, isValid: false);
        }
    }
}
