namespace ScriptCs
{
    using System.Collections.Generic;

    public interface IDependenciesPreProcessor
    {
        IEnumerable<string> GetDependencies(string path);
    }
}
