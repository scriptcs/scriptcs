using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ExtendedScriptHostFactory : ScriptHostFactory
    {
        public override IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new ExtendedScriptHost(scriptPackManager, new ScriptEnvironment(scriptArgs));
        }
    }
}
