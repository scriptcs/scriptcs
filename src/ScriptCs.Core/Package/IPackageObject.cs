using System.Collections.Generic;
using System.Runtime.Versioning;

namespace ScriptCs.Package
{
    public interface IPackageObject
    {
        string Id { get; }
        string Version { get; }
        string FullName { get; }
        IEnumerable<string> GetCompatibleDlls(FrameworkName frameworkName);
    }
}