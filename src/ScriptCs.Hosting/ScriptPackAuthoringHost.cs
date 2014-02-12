using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ScriptPackAuthoringHost : ScriptHost
    {
        public ScriptPackAuthoringHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment) : base(scriptPackManager, environment)
        {
        }

        public IScriptPackSettingsReferences ScriptPack<TScriptPackContext>() where TScriptPackContext : IScriptPackContext
        {
            return new ScriptPackSettings(typeof(TScriptPackContext));
        }
    }
}
