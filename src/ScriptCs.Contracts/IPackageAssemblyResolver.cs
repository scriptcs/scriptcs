using System;
using System.Collections.Generic;
using ScriptCs.Package;

namespace ScriptCs.Contracts
{
    public interface IPackageAssemblyResolver
    {
        void SavePackages(Action<string> output);
        IEnumerable<IPackageReference> GetPackages(string workingDirectory);
        IEnumerable<string> GetAssemblyNames(string workingDirectory, Action<string> outputCallback = null);
    }
}