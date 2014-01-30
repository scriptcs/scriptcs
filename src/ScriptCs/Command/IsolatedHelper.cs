using System;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    [Serializable]
    public class IsolatedHelper : IIsolatedHelper
    {
        public ScriptCsArgs CommandArgs { get; set; }
        public string[] AssemblyPaths { get; set; }
        public string Script { get; set; }
        public string[] ScriptArgs { get; set; }
        public ScriptResult Result { get; set; }

        public void Execute()
        {
            var scriptServicesFactory = new ScriptServicesFactory(CommandArgs);
            var scriptServiceRoot = scriptServicesFactory.Create();
            scriptServiceRoot.Logger.Debug("Creating isolated ScriptServiceRoot");
            var executor = scriptServiceRoot.Executor;
            executor.Initialize(AssemblyPaths, scriptServiceRoot.ScriptPackResolver.GetPacks());
            Result = executor.Execute(Script, ScriptArgs);
            executor.Terminate();
        }
    }
}