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
                return new ScriptExecuteCommand(args.ScriptName, _scriptServiceRoot.FileSystem, 
                    _scriptServiceRoot.PackageAssemblyResolver, _scriptServiceRoot.Executor, _scriptServiceRoot.ScriptPackResolver);
            }

            if (args.Install != null)
            {
                return new InstallCommand(args.Install, args.AllowPreReleaseFlag, _scriptServiceRoot.FileSystem, 
                    _scriptServiceRoot.PackageAssemblyResolver, _scriptServiceRoot.PackageInstaller);
            }

            return new InvalidCommand();
        }
    }
}