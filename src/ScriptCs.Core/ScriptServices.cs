using System.Collections.Generic;
using ScriptCs.Contracts;
using ScriptCs.Contracts.Logging;

namespace ScriptCs
{
    public class ScriptServices
    {
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
