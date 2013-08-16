namespace ScriptCs.Contracts
{
    public interface IScriptPackManager
    {
        TContext Get<TContext>() where TContext : IScriptPackContext;
    }
}