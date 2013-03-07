using System.Runtime.Versioning;

namespace ScriptCs.Package
{
    public interface IPackageReference
    {
        string PackageId { get; }
        FrameworkName FrameworkName { get; }
    }
}