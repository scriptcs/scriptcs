using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public interface IScriptServiceConfiguration
    {
        
    }

    public interface IScriptServiceConfiguration<out TConfig> : IScriptServiceConfiguration where TConfig : IScriptServiceConfiguration<TConfig>
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
    }

}