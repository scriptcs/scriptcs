using ScriptCs.Contracts;
using ScriptCs.Logging;

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

        ILog Logger { get; }

        IAppDomainAssemblyResolver GetAppDomainAssemblyResolver();

        IAssemblyUtility GetAssemblyUtility();
    }
}