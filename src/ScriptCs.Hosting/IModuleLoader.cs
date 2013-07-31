namespace ScriptCs
{
    public interface IModuleLoader
    {
        void Load(IModuleConfiguration config, string modulePackagesPath, string extension, params string[] moduleNames);
    }
}