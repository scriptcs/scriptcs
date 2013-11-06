using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptHostFactory<TScriptHost>
    {
        TScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs);
    }
}
