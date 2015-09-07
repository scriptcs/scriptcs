using System;
using System.IO;
using System.Linq;
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
        private readonly IScriptLibraryComposer _composer;

        public ExecuteScriptCommand(
            string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILogProvider logProvider,
            IAssemblyResolver assemblyResolver,
            IFileSystemMigrator fileSystemMigrator,
            IScriptLibraryComposer composer
            )
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("scriptExecutor", scriptExecutor);
            Guard.AgainstNullArgument("scriptPackResolver", scriptPackResolver);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("assemblyResolver", assemblyResolver);
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);
            Guard.AgainstNullArgument("composer", composer);

            _script = script;
            ScriptArgs = scriptArgs;
            _fileSystem = fileSystem;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            _logger = logProvider.ForCurrentType();
            _assemblyResolver = assemblyResolver;
            _fileSystemMigrator = fileSystemMigrator;
            _composer = composer;
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

                _composer.Compose(workingDirectory);

                _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks(), ScriptArgs);

                // HACK: This is a (dirty) fix for #1086. This might be a temporary solution until some further refactoring can be done. 
                _scriptExecutor.ScriptEngine.CacheDirectory = Path.Combine(workingDirectory ?? _fileSystem.CurrentDirectory, _fileSystem.DllCacheFolder);
                var scriptResult = _scriptExecutor.Execute(_script, ScriptArgs);
                var commandResult = Inspect(scriptResult);
                _scriptExecutor.Terminate();
                return commandResult;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error executing script '{0}'", ex, _script);
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
                _logger.ErrorException("Script compilation failed.", ex);
                return CommandResult.Error;
            }

            if (result.ExecuteExceptionInfo != null)
            {
                var ex = result.ExecuteExceptionInfo.SourceException;
                _logger.ErrorException("Script execution failed.", ex);
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
