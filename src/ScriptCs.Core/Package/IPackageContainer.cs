using System.Collections.Generic;

namespace ScriptCs.Package
{
    public interface IPackageContainer
    {
        IEnumerable<IPackageReference> FindReferences(string path);
        IPackageObject FindPackage(string path, string packageId);
    }
}