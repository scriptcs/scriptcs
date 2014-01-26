using System;
using System.Reflection;

using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class InMemoryAssemblyLoader : IAssemblyLoader
    {
        private readonly ILog _logger;

        public InMemoryAssemblyLoader(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _logger = logger;
        }

        public bool ShouldCompile()
        {
            return true;
        }

        public Assembly LoadFromCache()
        {
            throw new NotImplementedException("Reaching this point indicates a bug. The RoslynScriptInMemoryEngine should never load the assembly from the cache.");
        }

        public Assembly Load(byte[] exeBytes, byte[] pdbBytes)
        {
            _logger.Debug("Loading assembly from memory.");

            // this is required for debugging. otherwise, the .dll is not related to the .pdb
            // there might be ways of doing this without "loading", haven't found one yet
            return AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
        }

        public void SetContext(string fileName, string cacheDirectory)
        {
        }
    }
}
