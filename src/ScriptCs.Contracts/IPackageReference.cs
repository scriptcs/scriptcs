using System;
using System.Runtime.Versioning;

namespace ScriptCs.Contracts
{
    public interface IPackageReference
    {
        string PackageId { get; }
        
        FrameworkName FrameworkName { get; }
        
        Version Version { get; set; }
        
        string SpecialVersion { get; }
    }
}