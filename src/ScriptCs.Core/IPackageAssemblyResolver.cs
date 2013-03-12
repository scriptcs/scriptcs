using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ScriptCs
{
    public interface IPackageAssemblyResolver
    {
        IEnumerable<string> GetAssemblyNames(string workingDirectory);
    }
}