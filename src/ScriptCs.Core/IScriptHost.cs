using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptHost 
    {
        ScriptEnvironment Env { get; }

        T Require<T>() where T : IScriptPackContext;
    }
}