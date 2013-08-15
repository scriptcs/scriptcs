using System.Reflection;

using Common.Logging;

namespace ScriptCs.Engine.Roslyn
{
    using System.IO;

    using ScriptCs.Contracts;

    public class RoslynScriptPersistentEngine : RoslynScriptCompilerEngine
    {
        private IFileSystem _fileSystem;

        public RoslynScriptPersistentEngine(IScriptHostFactory scriptHostFactory, ILog logger, IFileSystem fileSystem)
            : base(scriptHostFactory, logger)
        {
            _fileSystem = fileSystem;
        }

        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            this.Logger.DebugFormat("Writing assembly to {0}.", FileName);

            if (!_fileSystem.DirectoryExists(BaseDirectory))
            {
                _fileSystem.CreateDirectory(BaseDirectory);
            }

            var dllName = FileName.Replace(Path.GetExtension(FileName), ".dll");
            var dllPath = Path.Combine(BaseDirectory, dllName);
            _fileSystem.WriteAllBytes(dllPath, exeBytes);

            this.Logger.DebugFormat("Loading assembly {0}.", dllPath);

            return Assembly.LoadFrom(dllPath);
        }
    }
}