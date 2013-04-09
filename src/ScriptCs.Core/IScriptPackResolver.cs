using ScriptCs.Contracts;
using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptPackResolver
    {
        IEnumerable<IScriptPack> GetPacks();
    }
}