using Autofac;

namespace ScriptCs
{
    public interface IInitializationContainerFactory
    {
        IAssemblyResolver GetAssemblyResolver();
        IModuleLoader GetModuleLoader();
        IFileSystem GetFileSystem();
    }

}