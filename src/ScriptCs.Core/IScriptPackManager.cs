using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptPackManager
    {
        TContext Get<TContext>() where TContext : IScriptPackContext;
    }
}