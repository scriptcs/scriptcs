using System;
using System.IO;
using System.Linq;
using Common.Logging;

using ScriptCs.Contracts;

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
        
        private readonly IFileSystemMigrator _fileSystemMigrator;

        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver,
            IFileSystemMigrator fileSystemMigrator)
        {
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);

            _script = script;
            _fileSystem = fileSystem;
            ScriptArgs = scriptArgs;
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
            catch (FileNotFoundException fnfex)
            {
                _logger.ErrorFormat("{0} - {1}", fnfex.Message, fnfex.FileName);
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
                _logger.Error(result.CompileExceptionInfo.SourceException.Message);
                _logger.Debug(result.CompileExceptionInfo.SourceException);
                return CommandResult.Error;
            }

            if (result.ExecuteExceptionInfo != null)
            {
                _logger.Error(result.ExecuteExceptionInfo.SourceException);
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
