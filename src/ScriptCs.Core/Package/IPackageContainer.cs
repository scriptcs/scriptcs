using System.Collections.Generic;

namespace ScriptCs.Package
{
    public interface IPackageContainer
    {
        IEnumerable<string> CreatePackageFile();
        IEnumerable<IPackageReference> FindReferences(string path);
        IPackageObject FindPackage(string path, IPackageReference packageReference);
    }
}