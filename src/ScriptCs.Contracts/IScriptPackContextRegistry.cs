using System;

namespace ScriptCs.Contracts
{
    public interface IScriptPackContextRegistry
    {
        void Register(Type context);
    }
}