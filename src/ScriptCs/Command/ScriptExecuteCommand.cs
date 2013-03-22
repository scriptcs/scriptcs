using System;
using System.Collections.Generic;
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
                var assemblyPaths = Enumerable.Empty<string>();

                var workingDirectory = _fileSystem.GetWorkingDirectory(_script);
                if (workingDirectory != null)
                {
                    assemblyPaths = GetAssemblyPaths(workingDirectory);
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

        private IEnumerable<string> GetAssemblyPaths(string workingDirectory)
        {
            var binFolder = Path.Combine(workingDirectory, "bin");

            var assemblyPaths = _fileSystem.EnumerateFiles(binFolder, "*.dll").ToList();
            foreach (var path in assemblyPaths.Select(Path.GetFileName))
            {
                Console.WriteLine("Found assembly reference: " + path);
            }

            return assemblyPaths;
        }
    }
}