using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    [Module("csharp")]
    public class CSharpModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            Guard.AgainstNullArgument("config", config);

            if (!config.Overrides.ContainsKey(typeof(IScriptEngine)))
            {
                var engineType = config.Cache ? typeof(CSharpPersistentEngine) : typeof(CSharpScriptEngine);
                engineType = config.Debug ? typeof(CSharpScriptInMemoryEngine) : engineType;
                engineType = config.IsRepl ? typeof(CSharpReplEngine) : engineType;
                config.Overrides[typeof(IScriptEngine)] = engineType;
            }
        }
    }
}
