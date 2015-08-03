using System;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptInMemoryEngine : RoslynScriptCompilerEngine
    {
        private readonly ILog _log;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public RoslynScriptInMemoryEngine(IScriptHostFactory scriptHostFactory, Common.Logging.ILog logger)
            : this(scriptHostFactory, new CommonLoggingLogProvider(logger))
        {
        }

        public RoslynScriptInMemoryEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider)
            : base(scriptHostFactory, logProvider)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _log = logProvider.ForCurrentType();
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
            _log.Debug("Loading assembly from memory.");

            // this is required for debugging. otherwise, the .dll is not related to the .pdb
            // there might be ways of doing this without "loading", haven't found one yet
            return AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
        }
    }
}