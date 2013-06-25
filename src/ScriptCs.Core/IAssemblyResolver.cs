using System.Collections.Generic;

namespace ScriptCs
{
    public interface IAssemblyResolver
    {
        IEnumerable<string> GetAssemblyPaths(string path);
    }
}
