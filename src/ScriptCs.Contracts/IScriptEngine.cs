using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }

        string CacheDirectory { get; set; }

        string FileName { get; set; }

        IScriptHostFactory ScriptHostFactory { get;}

        ScriptResult Execute(
            string code,
            string[] scriptArgs,
            AssemblyReferences references,
            IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession);
    }
}