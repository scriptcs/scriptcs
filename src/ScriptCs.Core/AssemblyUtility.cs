using System;
using System.Reflection;

using ScriptCs.Contracts;

namespace ScriptCs
{
    using System.IO;

    public class AssemblyUtility : IAssemblyUtility 
    {
        public bool IsManagedAssembly(string path)
        {
            if (!Path.IsPathRooted(path) && !(path.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) || 
                path.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

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