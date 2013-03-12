using System.Collections.Generic;

namespace ScriptCs
{
    public interface IPackageAssemblyResolver
    {
        IEnumerable<string> GetAssemblyNames(string workingDirectory);
    }
}