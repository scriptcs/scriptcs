﻿using System;
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
            var binFolder = Path.Combine(workingDirectory, "bin");

            if (!_fileSystem.DirectoryExists(binFolder))
                _fileSystem.CreateDirectory(binFolder);

            var assemblyPaths = 
                _fileSystem.EnumerateFiles(binFolder, "*.dll")
                .Union(_fileSystem.EnumerateFiles(binFolder, "*.exe"))
                .ToList();
                        
            foreach (var path in assemblyPaths.Select(Path.GetFileName))
            {
                Console.WriteLine("Found assembly reference: " + path);
            }

            return assemblyPaths;
        }
    }
}