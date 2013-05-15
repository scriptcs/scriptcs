using System.Reflection;
namespace ScriptCs
{
    public class Assembly : IAssembly
    {
        public AssemblyName GetAssemblyName(string path)
        {
            return AssemblyName.GetAssemblyName(path);
        }
    }
}
