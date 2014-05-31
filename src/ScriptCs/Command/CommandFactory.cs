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
                throw new ArgumentException("The file system provided by the initialization services provided by the script services builder has a null packages file.");
            }

            if (_fileSystem.PackagesFolder == null)
            {
                throw new ArgumentException("The file system provided by the initialization services provided by the script services builder has a null package folder.");
            }
        }

        public ICommand CreateCommand(ScriptCsArgs args, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("args", args);

            var logger = _initializationServices.Logger;
            var packageAssemblyResolver = _initializationServices.GetPackageAssemblyResolver();
            ScriptServices scriptServices = null;

            if (args.Help)
            {
                return new ShowUsageCommand(logger, isValid: true);
            }

            if (args.Global)
            {
                var currentDir = _fileSystem.ModulesFolder;
                if (!_fileSystem.DirectoryExists(currentDir))
                {
                    _fileSystem.CreateDirectory(currentDir);
                }

                _fileSystem.CurrentDirectory = currentDir;
            }

            _initializationServices.GetInstallationProvider().Initialize();

            if (args.Repl)
            {
                scriptServices = _scriptServicesBuilder.Build();
                var replCommand = new ExecuteReplCommand(
                    args.ScriptName,
                    scriptArgs,
                    scriptServices.FileSystem,
                    scriptServices.ScriptPackResolver,
                    scriptServices.Engine,
                    scriptServices.FilePreProcessor,
                    scriptServices.ObjectSerializer,
                    scriptServices.Logger,
                    scriptServices.Console,
                    scriptServices.AssemblyResolver,
                    scriptServices.ReplCommands);

                return replCommand;
            }

            if (args.ScriptName != null)
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
                        packageAssemblyResolver,
                        _initializationServices.GetPackageInstaller(),
                        logger);

                    var executeCommand = new DeferredCreationCommand<IScriptCommand>(() =>
                    {
                        scriptServices = ScriptServicesBuilderFactory.Create(args, scriptArgs).Build();
                        return CreateScriptCommand(args, scriptArgs, scriptServices);
                    });

                    return new CompositeCommand(installCommand, executeCommand);
                }

                return CreateScriptCommand(args, scriptArgs, _scriptServicesBuilder.Build());
            }

            if (args.Clean)
            {
                var saveCommand = new SaveCommand(packageAssemblyResolver, _fileSystem, logger);

                if (args.Global)
                {
                    var currentDirectory = _fileSystem.ModulesFolder;
                    _fileSystem.CurrentDirectory = currentDirectory;
                    if (!_fileSystem.DirectoryExists(currentDirectory))
                    {
                        _fileSystem.CreateDirectory(currentDirectory);
                    }
                }

                var cleanCommand = new CleanCommand(
                    args.ScriptName,
                    _fileSystem,
                    logger);

                return new CompositeCommand(saveCommand, cleanCommand);
            }

            if (args.Save)
            {
                return new SaveCommand(packageAssemblyResolver, _fileSystem, logger);
            }

            if (args.Version)
            {
                return new VersionCommand(_scriptServicesBuilder.ConsoleInstance);
            }

            if (args.Install != null)
            {
                var installCommand = new InstallCommand(
                    args.Install,
                    args.PackageVersion,
                    args.AllowPreRelease,
                    _fileSystem,
                    packageAssemblyResolver,
                    _initializationServices.GetPackageInstaller(),
                    logger);

                var saveCommand = new SaveCommand(packageAssemblyResolver, _fileSystem, logger);

                return new CompositeCommand(installCommand, saveCommand);
            }

            return new ShowUsageCommand(logger, isValid: false);
        }

        private static IScriptCommand CreateScriptCommand(
            ScriptCsArgs args, string[] scriptArgs, ScriptServices scriptServices)
        {
            return args.Watch
                ? (IScriptCommand)new WatchScriptCommand(
                    args,
                    scriptArgs,
                    scriptServices.Console,
                    scriptServices.FileSystem,
                    scriptServices.Logger)
                : (IScriptCommand)new ExecuteScriptCommand(
                    args.ScriptName,
                    scriptArgs,
                    scriptServices.FileSystem,
                    scriptServices.Executor,
                    scriptServices.ScriptPackResolver,
                    scriptServices.Logger,
                    scriptServices.AssemblyResolver);
        }
    }
}