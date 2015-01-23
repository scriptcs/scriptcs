using System;
using System.IO;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly ILog _logger;
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly IFileSystemMigrator _fileSystemMigrator;

        public ExecuteScriptCommand(
            string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver,
            IFileSystemMigrator fileSystemMigrator)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("scriptExecutor", scriptExecutor);
            Guard.AgainstNullArgument("scriptPackResolver", scriptPackResolver);
            Guard.AgainstNullArgument("logger", logger);
            Guard.AgainstNullArgument("assemblyResolver", assemblyResolver);
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);

            _script = script;
            ScriptArgs = scriptArgs;
            _fileSystem = fileSystem;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            _logger = logger;
            _assemblyResolver = assemblyResolver;
            _fileSystemMigrator = fileSystemMigrator;
        }

        public string[] ScriptArgs { get; private set; }

        public CommandResult Execute()
        {
            try
            {
                _fileSystemMigrator.Migrate();

                var assemblyPaths = Enumerable.Empty<string>();
                var workingDirectory = _fileSystem.GetWorkingDirectory(_script);
                if (workingDirectory != null)
                {
                    assemblyPaths = _assemblyResolver.GetAssemblyPaths(workingDirectory);
                }

                _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks(), ScriptArgs);
                var scriptResult = _scriptExecutor.Execute(_script, ScriptArgs);
                var commandResult = Inspect(scriptResult);
                _scriptExecutor.Terminate();
                return commandResult;
            }
            catch (FileNotFoundException ex)
            {
                _logger.ErrorFormat("{0} - '{1}'.", ex, ex.Message, ex.FileName);
                return CommandResult.Error;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return CommandResult.Error;
            }
        }

        private CommandResult Inspect(ScriptResult result)
        {
            if (result == null)
            {
                return CommandResult.Error;
            }

            if (result.CompileExceptionInfo != null)
            {
                var ex = result.CompileExceptionInfo.SourceException;
                _logger.ErrorFormat("Script compilation failed: {0}.", ex, ex.Message);
                return CommandResult.Error;
            }

            if (result.ExecuteExceptionInfo != null)
            {
                var ex = result.ExecuteExceptionInfo.SourceException;
                _logger.ErrorFormat("Script execution failed: {0}.", ex, ex.Message);
                return CommandResult.Error;
            }

            if (!result.IsCompleteSubmission)
            {
                _logger.Error("The script is incomplete.");
                return CommandResult.Error;
            }

            return CommandResult.Success;
        }
    }
}
