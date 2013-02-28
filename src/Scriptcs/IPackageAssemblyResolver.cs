using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Scriptcs
{
    [InheritedExport]
    public interface IPackageAssemblyResolver
    {
        IEnumerable<string> GetAssemblyNames();
    }
}