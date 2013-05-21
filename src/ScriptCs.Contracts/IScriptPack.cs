using System.ComponentModel.Composition;

namespace ScriptCs.Contracts
{
    [InheritedExport]
    public interface IScriptPack
    {
        void Initialize(IScriptPackSession session);

        IScriptPackContext GetContext();

        void Terminate();
    }

    public interface IScriptPack<TContext> : IScriptPack where TContext : IScriptPackContext
    {
        [Import]
        TContext Context { get; set; }
    }
}