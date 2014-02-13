using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptedScriptPackLoader
    {
        IEnumerable<Tuple<String, ScriptResult>> Load();
    }
}