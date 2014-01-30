using System;
using System.IO;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        private readonly IAssemblyResolver _assemblyResolver;
        protected readonly IFileSystem FileSystem;
        protected readonly ILog Logger;
        protected readonly string Script;
        private readonly IScriptExecutor _scriptExecutor;

        private readonly IScriptPackResolver _scriptPackResolver;

        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver)
        {
            Script = script;
            FileSystem = fileSystem;
            ScriptArgs = scriptArgs;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            Logger = logger;
            _assemblyResolver = assemblyResolver;
        }

        public string[] ScriptArgs { get; private set; }

        public CommandResult Execute()
        {
            try
            {
                string[] assemblyPaths = null;

                string workingDirectory = FileSystem.GetWorkingDirectory(Script);
                if (workingDirectory != null)
                {
                    assemblyPaths = _assemblyResolver.GetAssemblyPaths(workingDirectory);
                }

                ScriptResult result = Execute(workingDirectory, assemblyPaths ?? new string[0]);

                if (result == null) return CommandResult.Error;

                if (result.CompileExceptionInfo != null)
                {
                    Logger.Error(result.CompileExceptionInfo.SourceException.Message);
                    Logger.Debug(result.CompileExceptionInfo.SourceException);
                    return CommandResult.Error;
                }

                if (result.ExecuteExceptionInfo != null)
                {
                    Logger.Error(result.ExecuteExceptionInfo.SourceException);
                    return CommandResult.Error;
                }

                return CommandResult.Success;
            }
            catch (FileNotFoundException fnfex)
            {
                Logger.ErrorFormat("{0} - {1}", fnfex.Message, fnfex.FileName);
                return CommandResult.Error;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return CommandResult.Error;
            }
        }


        protected virtual ScriptResult Execute(string workingDirectory, string[] assemblyPaths)
        {
            _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks(), ScriptArgs);
            ScriptResult result = _scriptExecutor.Execute(Script, ScriptArgs);
            _scriptExecutor.Terminate();
            return result;
        }
    }
}