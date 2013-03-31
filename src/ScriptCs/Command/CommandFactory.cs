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
                var executeCommand = new ExecuteScriptCommand(
                    args.ScriptName, 
                    _scriptServiceRoot.FileSystem, 
                    _scriptServiceRoot.Executor,
                    _scriptServiceRoot.ScriptPackResolver,
                    _scriptServiceRoot.Logger);

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
                    args.AllowPreReleaseFlag,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.PackageInstaller,
                    _scriptServiceRoot.Logger);

                var restoreCommand = new RestoreCommand(
                    args.Install,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.Logger);

                return new CompositeCommand(installCommand, restoreCommand);
            }

            if (args.Clean)
            {
                var cleanCommand = new CleanCommand(
                    args.ScriptName,
                    _scriptServiceRoot.FileSystem,
                    _scriptServiceRoot.PackageAssemblyResolver,
                    _scriptServiceRoot.Logger);

                return cleanCommand;
            }

            if (args.Version)
            {
                return new VersionCommand();
            }

            return new InvalidCommand(_scriptServiceRoot.Logger);
        }
    }
}