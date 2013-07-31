using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IScriptHost {
        string[] ScriptArgs { get; }

        T Require<T>() where T : IScriptPackContext;
    }
}