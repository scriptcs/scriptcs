using System.Collections.Generic;

namespace ScriptCs
{
    public interface IAssemblyResolver
    {
        string[] GetAssemblyPaths(string path);
    }
}
