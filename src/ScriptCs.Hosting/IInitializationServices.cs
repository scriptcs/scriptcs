using Autofac;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IInitializationServices
    {
        IAssemblyResolver GetAssemblyResolver();
        IModuleLoader GetModuleLoader();
        IFileSystem GetFileSystem();
    }

}