using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IPackageContainer
    {
        IEnumerable<string> CreatePackageFile();

        IEnumerable<IPackageReference> FindReferences(string path);

        IPackageObject FindPackage(string path, IPackageReference packageReference);
    }
}