using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public interface IServiceOverrides
    {
        
    }

    public interface IServiceOverrides<out TConfig> : IServiceOverrides where TConfig : IServiceOverrides<TConfig>
    {
        TConfig ScriptExecutor<T>() where T : IScriptExecutor;
        TConfig ScriptEngine<T>() where T : IScriptEngine;
        TConfig ScriptHostFactory<T>() where T : IScriptHostFactory;
        TConfig ScriptPackManager<T>() where T : IScriptPackManager;
        TConfig ScriptPackResolver<T>() where T : IScriptPackResolver;
        TConfig InstallationProvider<T>() where T : IInstallationProvider;
        TConfig FileSystem<T>() where T : IFileSystem;
        TConfig AssemblyUtility<T>() where T : IAssemblyUtility;
        TConfig PackageContainer<T>() where T : IPackageContainer;
        TConfig FilePreProcessor<T>() where T : IFilePreProcessor;
        TConfig PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver;
        TConfig AssemblyResolver<T>() where T : IFilePreProcessor;
        TConfig AddLineProcessor<T>() where T : ILineProcessor;
    }

}