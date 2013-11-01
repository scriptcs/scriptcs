using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptPersistentEngine : RoslynScriptCompilerEngine
    {
        private readonly IFileSystem _fileSystem;

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

            // the assembly is automatically loaded into the AppDomain when compiled
            // just need to find and return it
            return AppDomain.CurrentDomain.GetAssemblies().LastOrDefault(x => x.FullName.StartsWith(RoslynAssemblyNameCharacter));
        }
    }
}