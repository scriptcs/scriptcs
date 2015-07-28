using System;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptServices
    {
        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public ScriptServices(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IScriptExecutor executor,
            IRepl repl,
            IScriptEngine engine,
            IFilePreProcessor filePreProcessor,
            IScriptPackResolver scriptPackResolver,
            IPackageInstaller packageInstaller,
            IObjectSerializer objectSerializer,
            Common.Logging.ILog logger,
            IAssemblyResolver assemblyResolver,
            IEnumerable<IReplCommand> replCommands,
            IFileSystemMigrator fileSystemMigrator,
            IConsole console = null,
            IInstallationProvider installationProvider = null,
            IScriptLibraryComposer scriptLibraryComposer = null
            )
            : this(
                fileSystem,
                packageAssemblyResolver,
                executor,
                repl,
                engine,
                filePreProcessor,
                scriptPackResolver,
                packageInstaller,
                objectSerializer,
                new CommonLoggingLogProvider(logger),
                assemblyResolver,
                replCommands,
                fileSystemMigrator,
                console,
                installationProvider,
                scriptLibraryComposer
            )
        {
        }

        public ScriptServices(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver,
            IScriptExecutor executor,
            IRepl repl,
            IScriptEngine engine,
            IFilePreProcessor filePreProcessor,
            IScriptPackResolver scriptPackResolver,
            IPackageInstaller packageInstaller,
            IObjectSerializer objectSerializer,
            ILogProvider logProvider,
            IAssemblyResolver assemblyResolver,
            IEnumerable<IReplCommand> replCommands,
            IFileSystemMigrator fileSystemMigrator,
            IConsole console = null,
            IInstallationProvider installationProvider = null,
            IScriptLibraryComposer scriptLibraryComposer = null
            )
        {
            FileSystem = fileSystem;
            PackageAssemblyResolver = packageAssemblyResolver;
            Executor = executor;
            Repl = repl;
            Engine = engine;
            FilePreProcessor = filePreProcessor;
            ScriptPackResolver = scriptPackResolver;
            PackageInstaller = packageInstaller;
            ObjectSerializer = objectSerializer;
            LogProvider = logProvider;
#pragma warning disable 618
            Logger = new ScriptCsLogger(logProvider.ForCurrentType());
#pragma warning restore 618
            Console = console;
            AssemblyResolver = assemblyResolver;
            InstallationProvider = installationProvider;
            ReplCommands = replCommands;
            FileSystemMigrator = fileSystemMigrator;
            ScriptLibraryComposer = scriptLibraryComposer;
        }

        public IFileSystem FileSystem { get; private set; }
        public IPackageAssemblyResolver PackageAssemblyResolver { get; private set; }
        public IScriptExecutor Executor { get; private set; }
        public IRepl Repl { get; private set; }
        public IScriptPackResolver ScriptPackResolver { get; private set; }
        public IPackageInstaller PackageInstaller { get; private set; }
        public IObjectSerializer ObjectSerializer { get; private set; }
        public ILogProvider LogProvider { get; private set; }
        
        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public Common.Logging.ILog Logger { get; private set; }
        
        public IScriptEngine Engine { get; private set; }
        public IFilePreProcessor FilePreProcessor { get; private set; }
        public IConsole Console { get; private set; }
        public IAssemblyResolver AssemblyResolver { get; private set; }
        public IInstallationProvider InstallationProvider { get; private set; }
        public IEnumerable<IReplCommand> ReplCommands { get; private set; }
        public IFileSystemMigrator FileSystemMigrator { get; private set; }
        public IScriptLibraryComposer ScriptLibraryComposer { get; private set; }
    }
}
