using System;
using System.Collections.Generic;
using ScriptCs.Package;

namespace ScriptCs
{
    public interface IPackageAssemblyResolver
    {
        IEnumerable<IPackageReference> GetPackages(string workingDirectory);
        IEnumerable<string> GetAssemblyNames(string workingDirectory, Action<string> outputCallback = null);
    }
}