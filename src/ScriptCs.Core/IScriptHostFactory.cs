using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptHostFactory
    {
        ScriptHost CreateScriptHost(IEnumerable<IScriptPackContext> contexts);
    }
}
