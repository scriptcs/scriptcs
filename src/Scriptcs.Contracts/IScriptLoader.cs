using System.Reflection;

namespace ScriptCs.Contracts
{
    public interface IAssemblyLoader
    {
        void SetContext(string fileName, string cacheDirectory);
        
        bool ShouldCompile();

        Assembly Load(byte[] exeBytes, byte[] pdbBytes);

        Assembly LoadFromCache();
    }
}
