using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using ScriptCs.Exceptions;

namespace ScriptCs.Command
{
    internal class ScriptExecuteCommand : IScriptCommand
    {
        private readonly string _script;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IScriptExecutor _scriptExecutor;
        private readonly IScriptPackResolver _scriptPackResolver;

        public ScriptExecuteCommand(string script, IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver, IScriptExecutor scriptExecutor, IScriptPackResolver scriptPackResolver)
        {
            _script = script;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _scriptExecutor = scriptExecutor;
            _scriptPackResolver = scriptPackResolver;
        }

        public int Execute()
        {
            try
            {
                var workingDirectory = _fileSystem.GetWorkingDirectory(_script);
                var paths = _packageAssemblyResolver.GetAssemblyNames(workingDirectory).ToList();
                foreach (var path in paths)
                {
                    Console.WriteLine("Found assembly reference: " + path);
                }

                _scriptExecutor.Execute(_script, paths, _scriptPackResolver.GetPacks());
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}