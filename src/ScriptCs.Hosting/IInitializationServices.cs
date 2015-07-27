using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public interface IInitializationServices
    {
        IAssemblyResolver GetAssemblyResolver();

        IModuleLoader GetModuleLoader();

        IFileSystem GetFileSystem();

        IInstallationProvider GetInstallationProvider();

        IPackageAssemblyResolver GetPackageAssemblyResolver();

        IPackageInstaller GetPackageInstaller();

        ILogProvider LogProvider { get; }

        IAppDomainAssemblyResolver GetAppDomainAssemblyResolver();

        IAssemblyUtility GetAssemblyUtility();
    }
}