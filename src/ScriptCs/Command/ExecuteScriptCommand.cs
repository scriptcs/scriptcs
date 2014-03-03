using System;
using System.Collections.Generic;
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
        
        private readonly IScriptedScriptPackLoader _scriptedScriptPackLoader;

        private readonly IScriptExecutor _scriptExecutor;

        private readonly IFileSystem _fileSystem;

        private readonly string _script;

        private readonly ILog _logger;

        private List<IScriptPack> _scriptedScriptPacks; 


        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver,
            IScriptedScriptPackLoader scriptedScriptPackLoader
            )
        {
            _script = script;
            _fileSystem = fileSystem;
            ScriptArgs = scriptArgs;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            _logger = logger;
            _assemblyResolver = assemblyResolver;
            _scriptedScriptPackLoader = scriptedScriptPackLoader;
            _scriptedScriptPacks = new List<IScriptPack>();

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

                var packs = _scriptPackResolver.GetPacks();
                _scriptExecutor.Initialize(assemblyPaths, packs, ScriptArgs);
                var loaderResult = _scriptedScriptPackLoader.Load(_scriptExecutor);
                foreach (var pack in loaderResult.ScriptPacks)
                {
                    _scriptExecutor.ScriptPackSession.AddScriptPack(pack);
                    _scriptExecutor.ScriptPackManager.AddContext(pack.GetContext());
                }

                var scriptResult = _scriptExecutor.Execute(_script, ScriptArgs);
                
                _scriptExecutor.Terminate();

                if (scriptResult == null) return CommandResult.Error;

                if (scriptResult.CompileExceptionInfo != null)
                {
                    _logger.Error(scriptResult.CompileExceptionInfo.SourceException.Message);
                    _logger.Debug(scriptResult.CompileExceptionInfo.SourceException);
                    return CommandResult.Error;
                }

                if (scriptResult.ExecuteExceptionInfo != null)
                {
                    _logger.Error(scriptResult.ExecuteExceptionInfo.SourceException);
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
