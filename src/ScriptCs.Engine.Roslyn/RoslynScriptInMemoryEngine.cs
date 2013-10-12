using System;
using System.Reflection;
using Common.Logging;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptInMemoryEngine : RoslynScriptCompilerEngine
    {
        public RoslynScriptInMemoryEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
        }

        protected override bool ShouldCompile()
        {
            return true;
        }

        protected override Assembly LoadAssemblyFromCache()
        {
            throw new NotImplementedException("Reaching this point indicates a bug. The RoslynScriptInMemoryEngine should never load the assembly from the cache.");
        }

        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            this.Logger.Debug("Loading assembly from memory.");

            // this is required for debugging. otherwise, the .dll is not related to the .pdb
            // there might be ways of doing this without "loading", haven't found one yet
            return AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
        }
    }
}