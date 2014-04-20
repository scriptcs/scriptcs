using System;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Mono
{
    [Module("mono", Extensions = "csx")]
    public class MonoModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            Console.WriteLine("Mono Engine initialized!");
            if (Type.GetType("Mono.Runtime") != null)
                config.ScriptEngine<MonoScriptEngine>();
        }
    }
}