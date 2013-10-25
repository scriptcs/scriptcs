using ScriptCs.Contracts;

namespace ScriptCs.Contracts
{
    public interface IScriptHost 
    {
        string[] ScriptArgs { get; }

        T Require<T>() where T : IScriptPackContext;
    }
}