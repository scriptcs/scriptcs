using System;
using System.IO;
using ScriptCs.Contracts;

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

            if (args.Global)
            {
                var currentDir = _scriptServices.FileSystem.ModulesFolder;
                if (!_scriptServices.FileSystem.DirectoryExists(currentDir))
                {
                    _scriptServices.FileSystem.CreateDirectory(currentDir);
                }

                _scriptServices.FileSystem.CurrentDirectory = currentDir;
            }

            _scriptServices.InstallationProvider.Initialize();

            if (args.Repl)
            {
                var replCommand = new ExecuteReplCommand(
                    args.ScriptName,
                    scriptArgs,
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
                var executeCommand = args.Isolated ?
                    new ExecuteIsolatedScriptCommand(
                    args.ScriptName,
                    scriptArgs,
                    _scriptServices.FileSystem,
                    _scriptServices.Executor,
                    _scriptServices.ScriptPackResolver,
                    _scriptServices.Logger,
                    _scriptServices.AssemblyResolver) { IsolatedHelper = new IsolatedHelper { CommandArgs = args, Script = args.ScriptName, ScriptArgs = scriptArgs } }
                    : new ExecuteScriptCommand(
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
                    _scriptServices.Logger);

                string currentDirectory = null;

                currentDirectory = _scriptServices.FileSystem.CurrentDirectory;

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
                    var currentDirectory = _scriptServices.FileSystem.ModulesFolder;
                    _scriptServices.FileSystem.CurrentDirectory = currentDirectory;
                    if (!_scriptServices.FileSystem.DirectoryExists(currentDirectory))
                    {
                        _scriptServices.FileSystem.CreateDirectory(currentDirectory);
                    }
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

    [Serializable]
    public class IsolatedHelper : IIsolatedHelper
    {
        public ScriptCsArgs CommandArgs { get; set; }
        public string[] AssemblyPaths { get; set; }
        public string Script { get; set; }
        public string[] ScriptArgs { get; set; }
        public ScriptResult Result { get; set; }

        public void Execute()
        {
            var scriptServiceRoot = CommandArgs.CreateServices();
            scriptServiceRoot.Logger.Debug("Creating isolated ScriptServiceRoot");
            var executor = scriptServiceRoot.Executor;
            executor.Initialize(AssemblyPaths, scriptServiceRoot.ScriptPackResolver.GetPacks());
            Result = executor.Execute(Script, ScriptArgs);
            executor.Terminate();
        }
    }
}
