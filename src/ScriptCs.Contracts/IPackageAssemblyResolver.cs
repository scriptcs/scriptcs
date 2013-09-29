using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IPackageAssemblyResolver
    {
        void SavePackages();
        IEnumerable<IPackageReference> GetPackages(string workingDirectory);
        IEnumerable<string> GetAssemblyNames(string workingDirectory);
    }
}