using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IAssemblyResolver
    {
        string[] GetAssemblyPaths(string path);
    }
}
