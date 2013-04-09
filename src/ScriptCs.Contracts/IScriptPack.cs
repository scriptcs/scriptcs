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
}
