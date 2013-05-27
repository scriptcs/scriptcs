using System;
using System.Linq;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : ScriptCommand
    {
        private readonly string _script;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;

        public ExecuteScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            IPackageAssemblyResolver packageAssemblyResolver,
            ILog logger,
            IAssemblyName assemblyName) : base(fileSystem, packageAssemblyResolver, assemblyName, logger)
        {
            _script = script;
            ScriptArgs = scriptArgs;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
        }

        public override CommandResult Execute()
        {
            try
            {
                var assemblyPaths = Enumerable.Empty<string>();

                var workingDirectory = FileSystem.GetWorkingDirectory(_script);
                if (workingDirectory != null)
                {
                    assemblyPaths = GetAssemblyPaths(workingDirectory);
                }
                _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks());
                _scriptExecutor.Execute(_script, ScriptArgs);
                _scriptExecutor.Terminate();

                return CommandResult.Success;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return CommandResult.Error;
            }
        }
    }
}
