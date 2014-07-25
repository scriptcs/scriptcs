using System.Reflection;

namespace ScriptCs.Contracts
{
    public interface IAssemblyUtility
    {
        bool IsManagedAssembly(string path);
        Assembly LoadFile(string path);
        Assembly Load(AssemblyName assemblyRef);
        AssemblyName GetAssemblyName(string path);
    }
}