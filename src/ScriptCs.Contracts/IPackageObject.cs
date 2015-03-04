using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace ScriptCs.Contracts
{
    public interface IPackageObject
    {
        string Id { get; }
        
        string TextVersion { get; }

        Version Version { get; }

        string FullName { get; }

        IEnumerable<string> GetCompatibleDlls(FrameworkName frameworkName);

        FrameworkName FrameworkName { get; }

        IEnumerable<IPackageObject> Dependencies { get; }

        IEnumerable<string> FrameworkAssemblies { get; }

        IEnumerable<string> GetContentFiles();
    }
}