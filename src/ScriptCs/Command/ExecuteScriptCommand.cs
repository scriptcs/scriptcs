using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;

        public ExecuteScriptCommand(string script, IFileSystem fileSystem, IScriptExecutor scriptExecutor, IScriptPackResolver scriptPackResolver)
        {
            _script = script;
            _fileSystem = fileSystem;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
        }

        public CommandResult Execute()
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
                return CommandResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return CommandResult.Error;
            }
        }

        private IEnumerable<string> GetAssemblyPaths(string workingDirectory)
        {
            var scriptAssembly = Path.GetFileNameWithoutExtension(_script) + ".dll";
            var binFolder = Path.Combine(workingDirectory, "bin");
            var assemblyPaths = new List<string>();

            if (_fileSystem.DirectoryExists(binFolder))
            {
                var paths = _fileSystem.EnumerateFiles(binFolder, "*.dll")
                                       .Union(_fileSystem.EnumerateFiles(binFolder, "*.exe"))
                                       .ToList();

                foreach (var path in paths)
                {
                    if (String.Compare(Path.GetFileName(path), scriptAssembly, true) != 0)
                    {
                        assemblyPaths.Add(path);
                        Console.WriteLine("Found assembly reference: " + path);
                    }
                }
            }
            else if (_scriptExecutor.CacheAssembly)
            {
                _fileSystem.CreateDirectory(binFolder);
            }

            return assemblyPaths;
        }
    }
}