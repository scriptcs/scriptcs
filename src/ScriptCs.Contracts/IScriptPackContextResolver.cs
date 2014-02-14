using System;
using ScriptCs.Contracts;

namespace ScriptCs.Contracts
{
    public interface IScriptPackContextResolver
    {
        IScriptPackContext Resolve(Type context);
    }
}
