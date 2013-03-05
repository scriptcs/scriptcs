using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ScriptCs
{
    [InheritedExport]
    public interface IPackageAssemblyResolver
    {
        IEnumerable<string> GetAssemblyNames();
    }
}