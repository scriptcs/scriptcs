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
        private const string RoslynAssemblyNameCharacter = "ℛ";

        public RoslynScriptPersistentEngine(IScriptHostFactory scriptHostFactory, ILog logger, IFileSystem fileSystem)
            : base(scriptHostFactory, logger)
        {
            _fileSystem = fileSystem;
        }

        protected override bool ShouldCompile()
        {
            var dllPath = GetDllTargetPath();
            var fileInfoAssembly = new FileInfo(dllPath);            
            var scriptPath = GetScriptTargetPath();
            var fileInfoScript = new FileInfo(scriptPath);
            var cachedAssemblyOutOfDate = fileInfoScript.LastWriteTimeUtc > fileInfoAssembly.LastWriteTimeUtc;

            return !_fileSystem.FileExists(dllPath) || cachedAssemblyOutOfDate;
        }

        protected override Assembly LoadAssembly(byte[] exeBytes, byte[] pdbBytes)
        {
            this.Logger.DebugFormat("Writing assembly to {0}.", FileName);

            if (!_fileSystem.DirectoryExists(CacheDirectory))
            {
                _fileSystem.CreateDirectory(CacheDirectory, true);
            }

            var dllPath = GetDllTargetPath();
            _fileSystem.WriteAllBytes(dllPath, exeBytes);

            Logger.DebugFormat("Loading assembly {0}.", dllPath);

            // the assembly is automatically loaded into the AppDomain when compiled
            // just need to find and return it
            return AppDomain.CurrentDomain.GetAssemblies().LastOrDefault(x => x.FullName.StartsWith(RoslynAssemblyNameCharacter));
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

        private string GetScriptTargetPath()
        {
          var scriptPath = Path.Combine(_fileSystem.CurrentDirectory, FileName);
          return scriptPath;
        }
    }
}