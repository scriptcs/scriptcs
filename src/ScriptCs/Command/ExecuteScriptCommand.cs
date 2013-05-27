using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Logging;
using System.Reflection;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IAssemblyName _assemblyName;

        private readonly ILog _logger;

        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyName assemblyName)
        {
            _script = script;
            ScriptArgs = scriptArgs;
            _fileSystem = fileSystem;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            _logger = logger;
            _assemblyName = assemblyName;
        }

        public string[] ScriptArgs { get; private set; }

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
                _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks());
                _scriptExecutor.Execute(_script, ScriptArgs);
                _scriptExecutor.Terminate();

                return CommandResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
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
                .Where(IsManagedAssembly)
                .ToList();
                        
            foreach (var path in assemblyPaths.Select(Path.GetFileName))
            {
                _logger.DebugFormat("Found assembly reference: {0}", path);
            }

            return assemblyPaths;
        }

        private bool IsManagedAssembly(string path)
        {
            try
            {
                _assemblyName.GetAssemblyName(path);
            }
            catch (BadImageFormatException)
            {
                return false;
            }
            return true;
        }
    }
}
