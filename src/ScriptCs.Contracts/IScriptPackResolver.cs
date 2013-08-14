using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptPackResolver
    {
        IEnumerable<IScriptPack> GetPacks();
    }
}