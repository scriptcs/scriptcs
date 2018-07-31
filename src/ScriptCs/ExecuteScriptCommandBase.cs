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
        protected string Script { get; private set; }
        protected IFileSystem FileSystem { get; private set; }
        protected IScriptExecutor ScriptExecutor { get; private set; }
        protected IScriptPackResolver _scriptPackResolver { get; private set; }
        protected ILog Logger { get; private set; }
        protected IAssemblyResolver AssemblyResolver { get; private set; }
        protected IScriptLibraryComposer Composer { get; private set; }

        public ExecuteScriptCommandBase(
            string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILogProvider logProvider,
            IAssemblyResolver assemblyResolver,
            IScriptLibraryComposer composer
            )
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("scriptExecutor", scriptExecutor);
            Guard.AgainstNullArgument("scriptPackResolver", scriptPackResolver);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("assemblyResolver", assemblyResolver);
            Guard.AgainstNullArgument("composer", composer);

            Script = script;
            ScriptArgs = scriptArgs;
            FileSystem = fileSystem;
            ScriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            Logger = logProvider.ForCurrentType();
            AssemblyResolver = assemblyResolver;
            Composer = composer;
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
                Logger.ErrorException("Script compilation failed.", ex);
                return CommandResult.Error;
            }

            if (result.ExecuteExceptionInfo != null)
            {
                var ex = result.ExecuteExceptionInfo.SourceException;
                Logger.ErrorException("Script execution failed.", ex);
                return CommandResult.Error;
            }

            if (!result.IsCompleteSubmission)
            {
                Logger.Error("The script is incomplete.");
                return CommandResult.Error;
            }

            return CommandResult.Success;
        }
    }
}
