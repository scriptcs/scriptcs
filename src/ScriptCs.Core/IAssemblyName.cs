using System.Reflection;
namespace ScriptCs
{
    public interface IAssembly
    {
        AssemblyName GetAssemblyName(string path);
    }
}
