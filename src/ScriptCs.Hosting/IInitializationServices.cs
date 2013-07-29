using Autofac;

namespace ScriptCs
{
    public interface IInitializationServices
    {
        IAssemblyResolver GetAssemblyResolver();
        IModuleLoader GetModuleLoader();
        IFileSystem GetFileSystem();
    }

}