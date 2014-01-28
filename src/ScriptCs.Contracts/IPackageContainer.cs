using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IPackageContainer
    {
        void CreatePackageFile();

        IEnumerable<IPackageReference> FindReferences(string path);

        IPackageObject FindPackage(string path, IPackageReference packageReference);
    }
}