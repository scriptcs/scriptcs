namespace ScriptCs.Contracts
{
    public interface IServiceOverrides { }

    public interface IServiceOverrides<out TConfig> : IServiceOverrides where TConfig : IServiceOverrides<TConfig>
    {
        TConfig ScriptExecutor<T>() where T : IScriptExecutor;

        TConfig Repl<T>() where T : IRepl;

        TConfig ScriptEngine<T>() where T : IScriptEngine;

        TConfig ScriptHostFactory<T>() where T : IScriptHostFactory;

        TConfig ScriptPackManager<T>() where T : IScriptPackManager;

        TConfig ScriptPackResolver<T>() where T : IScriptPackResolver;

        TConfig InstallationProvider<T>() where T : IInstallationProvider;

        TConfig FileSystem<T>() where T : IFileSystem;

        TConfig AssemblyUtility<T>() where T : IAssemblyUtility;

        TConfig ObjectSerializer<T>() where T : IObjectSerializer;

        TConfig PackageContainer<T>() where T : IPackageContainer;

        TConfig PackageInstaller<T>() where T : IPackageInstaller;

        TConfig FilePreProcessor<T>() where T : IFilePreProcessor;

        TConfig PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver;

        TConfig AssemblyResolver<T>() where T : IAssemblyResolver;

        TConfig LineProcessor<T>() where T : ILineProcessor;

        TConfig Console<T>() where T : IConsole;
    }
}
