using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IScriptedScriptPackLoader
    {
        ScriptedScriptPackLoadResult Load(IScriptExecutor executor);
    }
}