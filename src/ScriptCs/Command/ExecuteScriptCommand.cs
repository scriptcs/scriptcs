using System;
using System.IO;
using System.Linq;
using Common.Logging;
using System.Reflection;

using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : IScriptCommand
    {
        protected readonly string _script;
        protected readonly IFileSystem _fileSystem;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IAssemblyResolver _assemblyResolver;

        protected readonly ILog _logger;

        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver)
        {
            _script = script;
            ScriptArgs = scriptArgs;
            _fileSystem = fileSystem;
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
                var workingDirectory = _fileSystem.GetWorkingDirectory(_script);
                string[] assemblyPaths = null;
                if (workingDirectory != null)
                {
                    assemblyPaths = _assemblyResolver.GetAssemblyPaths(workingDirectory, _script);
                }

                var result = Execute(workingDirectory, assemblyPaths ?? new string[0]);

                if (result == null) return CommandResult.Error;

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

                return CommandResult.Success;
            }
            catch (FileNotFoundException fnfex)
            {
                _logger.ErrorFormat("{0} - {1}", fnfex.Message, fnfex.FileName);
                return CommandResult.Error;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return CommandResult.Error;
            }
        }

        protected virtual ScriptResult Execute(string workingDirectory, string[] assemblyPaths)
        {
            _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks());
            var result = _scriptExecutor.Execute(_script, ScriptArgs);
            _scriptExecutor.Terminate();
            return result;
        }
    }
}
