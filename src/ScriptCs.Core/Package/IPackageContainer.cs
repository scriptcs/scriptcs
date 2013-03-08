using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ScriptCs.Package
{
    [InheritedExport]
    public interface IPackageContainer
    {
        IEnumerable<IPackageReference> FindReferences(string path);
        IPackageObject FindPackage(string path, string packageId);
    }
}