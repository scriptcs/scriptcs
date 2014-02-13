using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptedScriptPackFinder
    {
        IEnumerable<string> GetScriptedScriptPacks(string workingDirectory);
    }
}