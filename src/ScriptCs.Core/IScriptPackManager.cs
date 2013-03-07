using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptPackManager
    {
        IEnumerable<IScriptPack> GetPacks();
    }
}