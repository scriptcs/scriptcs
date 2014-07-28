using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IAssemblyResolver
    {
        IEnumerable<string> GetAssemblyPaths(string path, bool binariesOnly = false);
    }
}
