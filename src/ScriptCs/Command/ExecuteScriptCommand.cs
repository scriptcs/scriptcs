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

                _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks(), ScriptArgs);
                var result = _scriptExecutor.Execute(_script, ScriptArgs);
                _scriptExecutor.Terminate();

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
    }
}
