using System;
using System.Linq;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        private readonly IScriptPackResolver _scriptPackResolver;

        private readonly IAssemblyResolver _assemblyResolver;

        private readonly IScriptExecutor _scriptExecutor;

        private readonly IFileSystem _fileSystem;

        private readonly string _script;

        private readonly ILog _logger;

        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver)
        {
            _script = script;
            _fileSystem = fileSystem;
            ScriptArgs = scriptArgs;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            _logger = logger;
            _assemblyResolver = assemblyResolver;
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
                    assemblyPaths = _assemblyResolver.GetAssemblyPaths(workingDirectory);
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
    }
}
