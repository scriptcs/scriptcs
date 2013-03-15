namespace ScriptCs
{
    public class ScriptServiceRoot
    {
        public ScriptServiceRoot(
            IFileSystem fileSystem,
            IPackageAssemblyResolver packageAssemblyResolver, 
            IScriptExecutor executor, 
            IScriptPackResolver scriptPackResolver )
        {
            FileSystem = fileSystem;
            PackageAssemblyResolver = packageAssemblyResolver;
            Executor = executor;
            ScriptPackResolver = scriptPackResolver;
        }

        public IFileSystem FileSystem { get; private set; }
        public IPackageAssemblyResolver PackageAssemblyResolver { get; private set; }
        public IScriptExecutor Executor { get; private set; }
        public IScriptPackResolver ScriptPackResolver { get; private set; }
    }
}