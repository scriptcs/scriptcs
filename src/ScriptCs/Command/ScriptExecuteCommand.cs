using System;
using System.IO;
using System.Linq;

namespace ScriptCs.Command
{
    internal class ScriptExecuteCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;

        public ScriptExecuteCommand(string script, IFileSystem fileSystem, IScriptExecutor scriptExecutor, IScriptPackResolver scriptPackResolver)
        {
            _script = script;
            _fileSystem = fileSystem;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
        }

        public int Execute()
        {
            try
            {
                var workingDirectory = _fileSystem.GetWorkingDirectory(_script);
                var binFolder = Path.Combine(workingDirectory, "bin");

                var assemblyPaths = _fileSystem.EnumerateFiles(binFolder, "*.dll").ToList();
                foreach (var path in assemblyPaths.Select(Path.GetFileName))
                {
                    Console.WriteLine("Found assembly reference: " + path);
                }

                _scriptExecutor.Execute(_script, assemblyPaths, _scriptPackResolver.GetPacks());
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}