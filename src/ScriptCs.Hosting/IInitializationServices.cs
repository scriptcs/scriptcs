namespace ScriptCs
{
    using ScriptCs.Contracts;

    public interface IInitializationServices
    {
        IAssemblyResolver GetAssemblyResolver();
        
        IModuleLoader GetModuleLoader();
        
        IFileSystem GetFileSystem();
    }
}