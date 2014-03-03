using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public interface IModuleLoader
    {
        void Load(IModuleConfiguration config, string[] modulePackagesPaths, string extension, params string[] moduleNames);
    }
}