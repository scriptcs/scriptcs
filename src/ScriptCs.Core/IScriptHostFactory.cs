using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [InheritedExport]
    public interface IScriptHostFactory
    {
        ScriptHost CreateScriptHost(IEnumerable<IScriptPackContext> contexts);
    }
}
