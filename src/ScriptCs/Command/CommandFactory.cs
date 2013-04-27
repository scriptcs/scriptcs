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

        public ICommand CreateCommand(ScriptCsArgs args)
        {
            if (args.ScriptName != null)
            {
                if (args.UncacheAssembly)
                    return new UncacheCommand(args.ScriptName, _scriptServiceRoot.FileSystem);

                _scriptServiceRoot.Executor.CacheAssembly = args.CacheAssembly;

                var executeCommand = new ExecuteScriptCommand(
                    args.ScriptName,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.Executor,
                    _scriptServiceRoot.ScriptPackResolver);

                if (args.Restore)
                {
                    var restoreCommand = new RestoreCommand(
                        args.ScriptName, 
                        _scriptServiceRoot.FileSystem, 
                        _scriptServiceRoot.PackageAssemblyResolver);

                    return new CompositeCommand(restoreCommand, executeCommand);
                }

                return executeCommand;
            }

            if (args.Install != null)
            {
                var installCommand = new InstallCommand(
                    args.Install,
                    args.AllowPreReleaseFlag,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.PackageInstaller);

                var restoreCommand = new RestoreCommand(
                    args.Install,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver);

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
                    _scriptServiceRoot.PackageAssemblyResolver);

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

            return new InvalidCommand();
        }
    }
}
