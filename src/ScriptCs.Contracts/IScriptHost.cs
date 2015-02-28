using ScriptCs.Contracts;

namespace ScriptCs.Contracts
{
    public interface IScriptHost 
    {
        T Require<T>() where T : IScriptPackContext;
        IScriptEnvironment Env { get; }
    }
}