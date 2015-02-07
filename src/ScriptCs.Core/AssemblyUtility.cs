using System;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class AssemblyUtility : IAssemblyUtility
    {
        public bool IsManagedAssembly(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
        }

        public Assembly LoadFile(string path)
        {
            return Assembly.LoadFile(path);
        }

        public Assembly Load(AssemblyName assemblyRef)
        {
            return Assembly.Load(assemblyRef);
        }

        public AssemblyName GetAssemblyName(string path)
        {
            return AssemblyName.GetAssemblyName(path);
        }
    }
}
