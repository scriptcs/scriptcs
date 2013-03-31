using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;

        private readonly ILog _logger;

        public ExecuteScriptCommand(string script, 
            IFileSystem fileSystem, 
            IScriptExecutor scriptExecutor, 
            IScriptPackResolver scriptPackResolver,
            ILog logger)
        {
            _script = script;
            _fileSystem = fileSystem;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            _logger = logger;
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
                _logger.Error(ex.Message);
                return CommandResult.Error;
            }
        }

        private IEnumerable<string> GetAssemblyPaths(string workingDirectory)
        {
            var binFolder = Path.Combine(workingDirectory, "bin");

            if (!_fileSystem.DirectoryExists(binFolder))
                _fileSystem.CreateDirectory(binFolder);

            var assemblyPaths = _fileSystem.EnumerateFiles(binFolder, "*.dll").ToList();
            foreach (var path in assemblyPaths.Select(Path.GetFileName))
            {
                _logger.InfoFormat("Found assembly reference: {0}", path);
            }

            return assemblyPaths;
        }
    }
}