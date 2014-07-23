using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IAppDomainAssemblyResolver
    {
        void AddAssemblyPaths(IEnumerable<string> assemblyPaths);
        void Initialize();
    }
}