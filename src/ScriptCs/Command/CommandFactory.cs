using System;
using System.IO;
using ScriptCs.Contracts;
using ScriptCs.Hosting;

namespace ScriptCs.Command
{
    public class CommandFactory
    {
        private readonly IScriptServicesBuilder _scriptServicesBuilder;
        private readonly IInitializationServices _initializationServices;
        private readonly IFileSystem _fileSystem;

        public CommandFactory(IScriptServicesBuilder scriptServicesBuilder)
        {
            Guard.AgainstNullArgument("scriptServicesBuilder", scriptServicesBuilder);

            _scriptServicesBuilder = scriptServicesBuilder;
            _initializationServices = _scriptServicesBuilder.InitializationServices;
            _fileSystem = _initializationServices.GetFileSystem();

            if (_fileSystem.PackagesFile == null)
            {
                throw new ArgumentException(
                    "The file system provided by the initialization services provided by the script services builder has a null packages file.", "scriptServicesBuilder");
            }

            if (_fileSystem.PackagesFolder == null)
            {
                throw new ArgumentException(
                    "The file system provided by the initialization services provided by the script services builder has a null package folder.", "scriptServicesBuilder");
            }
        }

        public ICommand CreateCommand(Config config, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("config", config);

            var scriptServices = _scriptServicesBuilder.Build();

            // HACK (Adam): This should not be the responsbility of the command factory
            // but now is not the time to fix this.
            // This should be addressed by a wider refactoring, i.e. https://github.com/scriptcs/scriptcs/issues/897
            scriptServices.FileSystemMigrator.Migrate();

            if (config.Global)
            {
                var currentDir = _fileSystem.GlobalFolder;
                if (!_fileSystem.DirectoryExists(currentDir))
                {
                    _fileSystem.CreateDirectory(currentDir);
                }

                _fileSystem.CurrentDirectory = currentDir;
            }

            _initializationServices.GetInstallationProvider().Initialize();

            if (config.Repl)
            {
                var explicitReplCommand = new ExecuteReplCommand(
                    config.ScriptName,
                    scriptArgs,
                    scriptServices.FileSystem,
                    scriptServices.ScriptPackResolver,
                    scriptServices.Repl,
                    scriptServices.LogProvider,
                    scriptServices.Console,
                    scriptServices.AssemblyResolver,
                    scriptServices.FileSystemMigrator,
                    scriptServices.ScriptLibraryComposer);

                return explicitReplCommand;
            }

            if (config.ScriptName != null)
            {
                var currentDirectory = _fileSystem.CurrentDirectory;
                var packageFile = Path.Combine(currentDirectory, _fileSystem.PackagesFile);
                var packagesFolder = Path.Combine(currentDirectory, _fileSystem.PackagesFolder);

                if (_fileSystem.FileExists(packageFile) && !_fileSystem.DirectoryExists(packagesFolder))
                {
                    var installCommand = new InstallCommand(
                        null,
                        null,
                        true,
                        _fileSystem,
                        _initializationServices.GetPackageAssemblyResolver(),
                        _initializationServices.GetPackageInstaller(),
                        scriptServices.ScriptLibraryComposer,
                        _initializationServices.LogProvider);

                    var executeCommand = new DeferredCreationCommand<IScriptCommand>(() =>
                        CreateScriptCommand(
                            config,
                            scriptArgs,
                            ScriptServicesBuilderFactory.Create(config, scriptArgs).Build()));

                    return new CompositeCommand(installCommand, executeCommand);
                }

                return CreateScriptCommand(config, scriptArgs, scriptServices);
            }

            if (config.Clean)
            {
                var saveCommand = new SaveCommand(
                    _initializationServices.GetPackageAssemblyResolver(),
                    _fileSystem,
                    _initializationServices.LogProvider);

                if (config.Global)
                {
                    var currentDirectory = _fileSystem.GlobalFolder;
                    _fileSystem.CurrentDirectory = currentDirectory;
                    if (!_fileSystem.DirectoryExists(currentDirectory))
                    {
                        _fileSystem.CreateDirectory(currentDirectory);
                    }
                }

                var cleanCommand = new CleanCommand(config.ScriptName, _fileSystem, _initializationServices.LogProvider);

                return new CompositeCommand(saveCommand, cleanCommand);
            }

            if (config.Save)
            {
                return new SaveCommand(
                    _initializationServices.GetPackageAssemblyResolver(),
                    _fileSystem,
                    _initializationServices.LogProvider);
            }

            if (config.PackageName != null)
            {
                var packageAssemblyResolver = _initializationServices.GetPackageAssemblyResolver();

                var installCommand = new InstallCommand(
                    config.PackageName,
                    config.PackageVersion,
                    config.AllowPreRelease,
                    _fileSystem,
                    packageAssemblyResolver,
                    _initializationServices.GetPackageInstaller(),
                    scriptServices.ScriptLibraryComposer,
                    _initializationServices.LogProvider);

                var saveCommand = new SaveCommand(packageAssemblyResolver, _fileSystem, _initializationServices.LogProvider);

                return new CompositeCommand(installCommand, saveCommand);
            }

            // NOTE (adamralph): no script name or command so assume REPL
            var replCommand = new ExecuteReplCommand(
                config.ScriptName,
                scriptArgs,
                scriptServices.FileSystem,
                scriptServices.ScriptPackResolver,
                scriptServices.Repl,
                scriptServices.LogProvider,
                scriptServices.Console,
                scriptServices.AssemblyResolver,
                scriptServices.FileSystemMigrator,
                scriptServices.ScriptLibraryComposer);

            return replCommand;
        }

        private static IScriptCommand CreateScriptCommand(
            Config config, string[] scriptArgs, ScriptServices scriptServices)
        {
            return config.Watch
                ? (IScriptCommand)new WatchScriptCommand(
                    config,
                    scriptArgs,
                    scriptServices.Console,
                    scriptServices.FileSystem,
                    scriptServices.LogProvider,
                    scriptServices.FileSystemMigrator)
                : new ExecuteScriptCommand(
                    config.ScriptName,
                    scriptArgs,
                    scriptServices.FileSystem,
                    scriptServices.Executor,
                    scriptServices.ScriptPackResolver,
                    scriptServices.LogProvider,
                    scriptServices.AssemblyResolver,
                    scriptServices.FileSystemMigrator,
                    scriptServices.ScriptLibraryComposer);
        }
    }
}
