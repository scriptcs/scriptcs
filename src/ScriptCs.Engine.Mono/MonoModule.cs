using ScriptCs.Contracts;

namespace ScriptCs.Engine.Mono
{
    [Module("mono")]
    public class MonoModule : IModule
    {
        public void Initialize(IModuleConfiguration config)
        {
            Guard.AgainstNullArgument("config", config);

            if (!config.Overrides.ContainsKey(typeof(IScriptEngine)))
            {
                config.ScriptEngine<MonoScriptEngine>();
            }
        }
    }
}
