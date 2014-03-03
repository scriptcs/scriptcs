using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public interface IInitializationServices
    {
        IAssemblyResolver GetAssemblyResolver();
        
        IModuleLoader GetModuleLoader();
        
        IFileSystem GetFileSystem();
    }
}