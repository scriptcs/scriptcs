using System;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Mono
{
    [Module(ModuleName)]
    public class MonoModule : IModule
    {
        public const string ModuleName = "mono";

        public void Initialize(IModuleConfiguration config)
        {
            Console.WriteLine("Mono Engine initialized!");
            if (!config.Overrides.ContainsKey(typeof(IScriptEngine)))
                config.ScriptEngine<MonoScriptEngine>();
        }
    }
}