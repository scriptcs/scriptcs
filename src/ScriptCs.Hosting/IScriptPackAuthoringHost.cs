using System;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public interface IScriptPackAuthoringHost
    {
        ScriptPackSettings ScriptPackSettings { get; set; }

        Type ScriptPackType { get; set; }

        IScriptPackSettingsReferences ScriptPack<T>();
    }
}