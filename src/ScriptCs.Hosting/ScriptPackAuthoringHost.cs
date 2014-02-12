using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ScriptPackAuthoringHost : ScriptHost, IScriptPackAuthoringHost
    {
        ScriptPackSettings IScriptPackAuthoringHost.ScriptPackSettings { get; set; }

        Type IScriptPackAuthoringHost.ScriptPackType { get; set; }

        public ScriptPackAuthoringHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment) : base(scriptPackManager, environment)
        {
        }

        public IScriptPackSettingsReferences ScriptPack<TScriptPack>()
        {
            var host = (IScriptPackAuthoringHost) this;

            if (typeof(IScriptPack).IsAssignableFrom(typeof(TScriptPack)))
            {
                host.ScriptPackType = typeof (TScriptPack);
                return null;
            }
            
            if (typeof (IScriptPackContext).IsAssignableFrom(typeof(TScriptPack)))
            {
                host.ScriptPackSettings = new ScriptPackSettings(typeof (TScriptPack));
                return host.ScriptPackSettings;
            }
                
            throw new ArgumentException(
                    string.Format("'{0}' is not a valid type as it does not implement IScriptPack or IScriptPackContext", typeof(TScriptPack)));
        }


    }
}
