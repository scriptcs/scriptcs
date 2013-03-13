namespace ScriptCs.Contracts
{
    public interface IScriptPack
    {
        void Initialize(IScriptPackSession session);
        IScriptPackContext GetContext(); 
        void Terminate();
    }
}
