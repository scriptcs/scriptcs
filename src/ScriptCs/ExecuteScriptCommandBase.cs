using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Command;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public abstract class ExecuteScriptCommandBase
    {
        protected readonly string _script;
        protected readonly IFileSystem _fileSystem;
        protected readonly IScriptExecutor _scriptExecutor;
        protected readonly IScriptPackResolver _scriptPackResolver;
        protected readonly ILog _logger;
        protected readonly IAssemblyResolver _assemblyResolver;
        protected readonly IFileSystemMigrator _fileSystemMigrator;
        protected readonly IScriptLibraryComposer _composer;

        public ExecuteScriptCommandBase(
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

        public abstract CommandResult Execute();

        protected CommandResult Inspect(ScriptResult result)
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
