namespace ScriptCs.Contracts
{
    public abstract class ScriptPack<TContext> : IScriptPack<TContext> where TContext : IScriptPackContext
    {
        public TContext Context { get; set; }

        public virtual void Initialize(IScriptPackSession session)
        {
        }

        public virtual IScriptPackContext GetContext()
        {
            return Context;
        }

        public virtual void Terminate()
        {
        }
    }
}