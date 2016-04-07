using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class CSharpPersistentEngine : CSharpScriptCompilerEngine
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILog _log;

        public CSharpPersistentEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider, IFileSystem fileSystem)
            : base(scriptHostFactory, logProvider)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);
            _log = logProvider.ForCurrentType();
            _fileSystem = fileSystem;
        }

        protected override bool ShouldCompile()
        {
            var dllPath = GetDllTargetPath();

            return !_fileSystem.FileExists(dllPath);
        }

        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            _log.DebugFormat("Writing assembly to {0}.", FileName);

            if (!_fileSystem.DirectoryExists(CacheDirectory))
            {
                _fileSystem.CreateDirectory(CacheDirectory, true);
            }

            var dllPath = GetDllTargetPath();
            _fileSystem.WriteAllBytes(dllPath, exeBytes);

            _log.DebugFormat("Loading assembly {0}.", dllPath);
            return LoadAssemblyFromCache();
        }

        protected override Assembly LoadAssemblyFromCache()
        {
            var dllPath = GetDllTargetPath();
            return Assembly.LoadFrom(dllPath);
        }

        private string GetDllTargetPath()
        {
            var dllName = FileName.Replace(Path.GetExtension(FileName), ".dll");
            var dllPath = Path.Combine(CacheDirectory, dllName);
            return dllPath;
        }
    }
}