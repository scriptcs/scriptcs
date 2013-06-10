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
            IAssemblyName assemblyName,
            IConsole console = null)
        {
            FileSystem = fileSystem;
            PackageAssemblyResolver = packageAssemblyResolver;
            Executor = executor;
            Engine = engine;
            FilePreProcessor = filePreProcessor;
            ScriptPackResolver = scriptPackResolver;
            PackageInstaller = packageInstaller;
            Logger = logger;
            Console = console;
            AssemblyName = assemblyName;
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
        public IAssemblyName AssemblyName { get; private set; }
    }
}
