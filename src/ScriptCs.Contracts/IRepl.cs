using System.Collections.Generic;
using System.Reflection;

namespace ScriptCs.Contracts
{
    public interface IRepl : IScriptExecutor
    {
        void Quit();
    }
}
