using System;
using ScriptCs.Contracts;

namespace ScriptCs.VisualBasic
{
    [Module("vb")]
    public class VisualBasicModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var engineType = config.IsRepl ? typeof(VisualBasicReplEngine) : typeof(VisualBasicScriptEngine);
            config.Overrides[typeof (IScriptEngine)] = engineType;
            config.LineProcessor<VisualBasicNamespaceLineProcessor>();
        }
    }
}