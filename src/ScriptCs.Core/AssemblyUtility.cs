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
    }
}