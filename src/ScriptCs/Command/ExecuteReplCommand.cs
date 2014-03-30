using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteReplCommand : IScriptCommand
    {
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly IEnumerable<IReplCommand> _replCommands;
        private readonly IFilePreProcessor _filePreProcessor;
        private readonly IObjectSerializer _serializer;
        private readonly IScriptEngine _scriptEngine;
        private readonly string _scriptName;
        private readonly string[] _scriptArgs;
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;
        private readonly ILog _logger;

        public ExecuteReplCommand(
            string scriptName,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptPackResolver scriptPackResolver,
            IScriptEngine scriptEngine,
            IFilePreProcessor filePreProcessor,
            IObjectSerializer serializer,
            ILog logger,
            IConsole console,
            IAssemblyResolver assemblyResolver, IEnumerable<IReplCommand> replCommands)
        {
            _scriptName = scriptName;
            _scriptArgs = scriptArgs;
            _fileSystem = fileSystem;
            _scriptPackResolver = scriptPackResolver;
            _scriptEngine = scriptEngine;
            _filePreProcessor = filePreProcessor;
            _serializer = serializer;
            _logger = logger;
            _console = console;
            _assemblyResolver = assemblyResolver;
            _replCommands = replCommands;
        }

        public string[] ScriptArgs { get; private set; }

        public CommandResult Execute()
        {
            _console.WriteLine("scriptcs (ctrl-c to exit)\r\n");
            var repl = new Repl(_scriptArgs, _fileSystem, _scriptEngine, _serializer, _logger, _console, _filePreProcessor, _replCommands);

            var workingDirectory = _fileSystem.CurrentDirectory;
            var assemblies = _assemblyResolver.GetAssemblyPaths(workingDirectory);
            var scriptPacks = _scriptPackResolver.GetPacks();

            repl.Initialize(assemblies, scriptPacks, ScriptArgs);

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

                _console.WriteLine();
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

            string line = null;
            
            try
            {
                line = _console.ReadLine();
            }
            catch
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                repl.Execute(line);
            }

            return true;
        }
    }
}
