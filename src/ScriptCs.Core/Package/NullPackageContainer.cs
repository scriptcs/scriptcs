using System.Collections.Generic;

namespace ScriptCs.Package
{
    public class NullPackageContainer : IPackageContainer
    {
        public IEnumerable<string> CreatePackageFile()
        {
            return new string[0];
        }

        public IEnumerable<IPackageReference> FindReferences(string path)
        {
            return new IPackageReference[0];
        }

        public IPackageObject FindPackage(string path, IPackageReference packageReference)
        {
            return null;
        }
    }
}