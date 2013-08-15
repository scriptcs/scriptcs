using System;
using System.Reflection;
using Common.Logging;

namespace ScriptCs.Engine.Roslyn
{
    using ScriptCs.Contracts;

    public class RoslynScriptInMemoryEngine : RoslynScriptCompilerEngine
    {
        public RoslynScriptInMemoryEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
        }
        
        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            this.Logger.Debug("Loading assembly from memory.");
            return AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
        }
    }
}