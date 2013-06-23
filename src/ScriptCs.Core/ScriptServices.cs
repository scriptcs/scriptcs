using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Package;

namespace ScriptCs
{
    public class ScriptServiceRoot
    {
        public ScriptServiceRoot(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver, 
            IScriptExecutor executor,
            IScriptEngine engine,
            IFilePreProcessor filePreProcessor,
            IScriptPackResolver scriptPackResolver, 
            IPackageInstaller packageInstaller,
            ILog logger,
            IAssemblyResolver assemblyResolver,
            IConsole console = null,
            IArgumentParser argumentParser = null, // todo: remove null
            IConfigFileParser configParser = null, // todo: remove null
            IArgumentHandler argumentHandler = null // todo: remove null
            )
        {
            FileSystem = fileSystem;
            PackageAssemblyResolver = packageAssemblyResolver;
            Executor = executor;
            Engine = engine;
            FilePreProcessor = filePreProcessor;
            ScriptPackResolver = scriptPackResolver;
            PackageInstaller = packageInstaller;
            Logger = logger;
            AssemblyResolver = assemblyResolver;
            ArgumentParser = argumentParser;
            ConfigParser = configParser;
            ArgumentHandler = argumentHandler;
            Console = console;
        }

        public IFileSystem FileSystem { get; private set; }
        public IPackageAssemblyResolver PackageAssemblyResolver { get; private set; }
        public IScriptExecutor Executor { get; private set; }
        public IScriptPackResolver ScriptPackResolver { get; private set; }
        public IPackageInstaller PackageInstaller { get; private set; }
        public ILog Logger { get; private set; }
        public IScriptEngine Engine { get; private set; }
        public IFilePreProcessor FilePreProcessor { get; private set; }
        public IConsole Console { get; private set; }
        public IAssemblyResolver AssemblyResolver { get; private set; }
        public IArgumentParser ArgumentParser { get; private set; }
        public IConfigFileParser ConfigParser { get; private set; }
        public IArgumentHandler ArgumentHandler { get; private set; }
    }
}
