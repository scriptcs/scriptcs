using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class DiskAssemblyLoader : IAssemblyLoader
    {
        private readonly IFileSystem _fileSystem;
        private const string RoslynAssemblyNameCharacter = "ℛ";
        private readonly ILog _logger;
        private string _cacheDirectory;
        private string _scriptFileName;

        public DiskAssemblyLoader(IScriptHostFactory scriptHostFactory, ILog logger, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public bool ShouldCompile()
        {
            var dllPath = GetDllTargetPath();

            return !_fileSystem.FileExists(dllPath);
        }

        public Assembly Load(byte[] exeBytes, byte[] pdbBytes)
        {
            _logger.DebugFormat("Writing assembly to {0}.", _scriptFileName);

            if (!_fileSystem.DirectoryExists(_cacheDirectory))
            {
                _fileSystem.CreateDirectory(_cacheDirectory, true);
            }

            var dllPath = GetDllTargetPath();
            _fileSystem.WriteAllBytes(dllPath, exeBytes);

            _logger.DebugFormat("Loading assembly {0}.", dllPath);

            // the assembly is automatically loaded into the AppDomain when compiled
            // just need to find and return it
            return AppDomain.CurrentDomain.GetAssemblies().LastOrDefault(x => x.FullName.StartsWith(RoslynAssemblyNameCharacter));
        }

        public Assembly LoadFromCache()
        {
            var dllPath = GetDllTargetPath();
            return Assembly.LoadFrom(dllPath);
        }

        private string GetDllTargetPath()
        {
            var dllName = _scriptFileName.Replace(Path.GetExtension(_scriptFileName), ".dll");
            var dllPath = Path.Combine(_cacheDirectory, dllName);
            return dllPath;
        }

        public void SetContext(string fileName, string cacheDirectory)
        {
            _scriptFileName = fileName;
            _cacheDirectory = cacheDirectory;
        }
    }
}
