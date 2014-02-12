using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public interface IExtendedScriptHost
    {
        ScriptPackSettings ScriptPackSettings { get; set; }

        Type ScriptPackType { get; set; }

        IScriptPackSettingsReferences ScriptPack<T>();
    }
}