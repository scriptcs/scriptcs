namespace ScriptCs.Contracts
{
    public interface IServiceOverrides { }

    public interface IServiceOverrides<out TConfig> : IServiceOverrides where TConfig : IServiceOverrides<TConfig>
    {
        TConfig WithScriptExecutor<T>() where T : IScriptExecutor;

        TConfig WithScriptEngine<T>() where T : IScriptEngine;

        TConfig WithScriptHostFactory<T>() where T : IScriptHostFactory;

        TConfig WithScriptPackManager<T>() where T : IScriptPackManager;

        TConfig WithScriptPackResolver<T>() where T : IScriptPackResolver;

        TConfig WithInstallationProvider<T>() where T : IInstallationProvider;

        TConfig WithFileSystem<T>() where T : IFileSystem;

        TConfig WithAssemblyUtility<T>() where T : IAssemblyUtility;

        TConfig WithPackageContainer<T>() where T : IPackageContainer;

        TConfig WithPackageInstaller<T>() where T : IPackageInstaller;

        TConfig WithFilePreProcessor<T>() where T : IFilePreProcessor;

        TConfig WithPackageAssemblyResolver<T>() where T : IPackageAssemblyResolver;

        TConfig WithAssemblyResolver<T>() where T : IAssemblyResolver;

        TConfig WithLineProcessor<T>() where T : ILineProcessor;
    }
}