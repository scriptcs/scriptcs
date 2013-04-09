using ScriptCs.Package;

namespace ScriptCs
{
    public class ScriptServiceRoot
    {
        public ScriptServiceRoot(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver, 
            IScriptExecutor executor, 
            IScriptPackResolver scriptPackResolver, IPackageInstaller packageInstaller)
        {
            FileSystem = fileSystem;
            PackageAssemblyResolver = packageAssemblyResolver;
            Executor = executor;
            ScriptPackResolver = scriptPackResolver;
            PackageInstaller = packageInstaller;
        }

        public IFileSystem FileSystem { get; private set; }
        public IPackageAssemblyResolver PackageAssemblyResolver { get; private set; }
        public IScriptExecutor Executor { get; private set; }
        public IScriptPackResolver ScriptPackResolver { get; private set; }
        public IPackageInstaller PackageInstaller { get; private set; }
    }
}