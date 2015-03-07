using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    [Module("roslyn")]
    public class RoslynModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            Guard.AgainstNullArgument("config", config);

            //if (!config.Overrides.ContainsKey(typeof(IScriptEngine)))
            //{
                //var engineType = config.Cache ? typeof(RoslynScriptPersistentEngine) : typeof(RoslynScriptEngine);
                //engineType = config.Debug ? typeof(RoslynScriptInMemoryEngine) : engineType;
                var engineType = config.IsRepl ? typeof(RoslynReplEngine) : typeof(RoslynScriptEngine);
                config.Overrides[typeof(IScriptEngine)] = engineType;
            //}
        }
    }
}
