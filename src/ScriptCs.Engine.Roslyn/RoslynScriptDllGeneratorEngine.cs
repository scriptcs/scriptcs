using Common.Logging;
using Roslyn.Scripting;
using System;
using System.Reflection;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    using System.IO;

    public class RoslynScriptDllGeneratorEngine : RoslynScriptCompilerEngine
    {
        private IFileSystem _fileSystem;

        public RoslynScriptDllGeneratorEngine(IScriptHostFactory scriptHostFactory, ILog logger, IFileSystem fileSystem)
            : base(scriptHostFactory, logger)
        {
            _fileSystem = fileSystem;
        }

        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            _logger.DebugFormat("Writing assembly to {0}.", FileName);
            var dllName = FileName.Replace(Path.GetExtension(FileName), ".dll");
            var dllPath = Path.Combine(this.BaseDirectory, dllName);
            _fileSystem.WriteAllBytes(dllPath, exeBytes);
            _logger.DebugFormat("Loading assembly {0}.", dllName);
            return Assembly.LoadFrom(dllPath);
>>>>>>> # Added RoslynScriptDllGeneratorEngine.cs which saves generated file to .dll
        }
    }
}