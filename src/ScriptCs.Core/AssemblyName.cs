namespace ScriptCs
{
    public class AssemblyName : IAssemblyName
    {
        public System.Reflection.AssemblyName GetAssemblyName(string path)
        {
            return System.Reflection.AssemblyName.GetAssemblyName(path);
        }
    }
}
