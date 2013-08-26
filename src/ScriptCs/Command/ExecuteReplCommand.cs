using System;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteReplCommand : IScriptCommand
    {
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly IFilePreProcessor _filePreProcessor;
        private readonly IScriptEngine _scriptEngine;
        private readonly string _scriptName;
        private readonly string[] _scriptArgs;
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;
        private readonly ILog _logger;
        private readonly ICompilationExceptionWriter _compilationExceptionWriter;

        public ExecuteReplCommand(
            string scriptName,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptPackResolver scriptPackResolver,
            IScriptEngine scriptEngine,
            IFilePreProcessor filePreProcessor,
            ILog logger,
            IConsole console,
            IAssemblyResolver assemblyResolver,
            ICompilationExceptionWriter compilationExceptionWriter)
        {
            _scriptName = scriptName;
            _scriptArgs = scriptArgs;
            _fileSystem = fileSystem;
            _scriptPackResolver = scriptPackResolver;
            _scriptEngine = scriptEngine;
            _filePreProcessor = filePreProcessor;
            _logger = logger;
            _console = console;
            _assemblyResolver = assemblyResolver;
            _compilationExceptionWriter = compilationExceptionWriter;
        }

        public string[] ScriptArgs { get; private set; }

        public CommandResult Execute()
        {
            _console.WriteLine("scriptcs (ctrl-c or blank to exit)\r\n");
            if (_compilationExceptionWriter != null)
            {
                _console.WriteLine("Compilation errors will be written to: " + _compilationExceptionWriter.CompilationExceptionFilePath + "\r\n");
            } 
            
            var repl = new Repl(_scriptArgs, _fileSystem, _scriptEngine, _logger, _console, _filePreProcessor, _compilationExceptionWriter);

            var workingDirectory = _fileSystem.CurrentDirectory;
            var assemblies = _assemblyResolver.GetAssemblyPaths(workingDirectory, string.Empty);
            var scriptPacks = _scriptPackResolver.GetPacks();

            repl.Initialize(assemblies, scriptPacks);

            try
            {
                if (!string.IsNullOrWhiteSpace(_scriptName))
                {
                    _logger.Info(string.Format("Loading preseeded script: {0}", _scriptName));
                    repl.Execute(string.Format("#load {0}", _scriptName));
                }

                while (ExecuteLine(repl))
                {
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return CommandResult.Error;              
            }

            repl.Terminate();
            return CommandResult.Success;
        }

        private bool ExecuteLine(Repl repl)
        {
            if (string.IsNullOrWhiteSpace(repl.Buffer))
            {
                _console.Write("> ");
            }

            var line = _console.ReadLine();
            if (line == string.Empty && string.IsNullOrWhiteSpace(repl.Buffer))
            {
                return false;
            }

            repl.Execute(line);
            return true;
        }
    }
}
