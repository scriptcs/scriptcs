using System;
using System.Linq;

using log4net;

namespace ScriptCs.Command
{
    internal class ScriptExecuteCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;

        private readonly ILog _logger;

        public ScriptExecuteCommand(string script, IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver, IScriptExecutor scriptExecutor, IScriptPackResolver scriptPackResolver, ILog logger)
        {
            _script = script;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
            this._logger = logger;
        }

        public int Execute()
        {
            try
            {
                var workingDirectory = _fileSystem.GetWorkingDirectory(_script);
                var paths = _packageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    _logger.InfoFormat("Found assembly reference: {0}", path);
                }

                _scriptExecutor.Execute(_script, paths, _scriptPackResolver.GetPacks());
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.Message);
                return -1;
            }
        }
    }
}