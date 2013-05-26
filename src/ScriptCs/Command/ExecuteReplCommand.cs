using System;

using Common.Logging;

namespace ScriptCs.Command
{
    internal class ExecuteReplCommand : ScriptCommand
    {
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IFilePreProcessor _filePreProcessor;
        private readonly IScriptEngine _scriptEngine;
        private readonly IConsole _console;

        public ExecuteReplCommand(
            IFileSystem fileSystem,
            IScriptPackResolver scriptPackResolver,
            IPackageAssemblyResolver packageAssemblyResolver,
            IScriptEngine scriptEngine,
            IFilePreProcessor filePreProcessor,
            ILog logger,
            IConsole console,
            IAssemblyName assemblyName) : base(fileSystem, packageAssemblyResolver, assemblyName, logger)
        {
            _scriptPackResolver = scriptPackResolver;
            _scriptEngine = scriptEngine;
            _filePreProcessor = filePreProcessor;
            _console = console;
        }

        public override CommandResult Execute()
        {
            _console.WriteLine("scriptcs (ctrl-c or blank to exit)\r\n");
            var repl = new Repl(FileSystem, _scriptEngine, Logger, _console, _filePreProcessor);

            var assemblies = GetAssemblyPaths(FileSystem.CurrentDirectory);
            var scriptPacks = _scriptPackResolver.GetPacks();

            repl.Initialize(assemblies, scriptPacks);

            try
            {
                while (ExecuteLine(repl)) { }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return CommandResult.Error;              
            }

            repl.Terminate();
            return CommandResult.Success;
        }

        private bool ExecuteLine(Repl repl)
        {
            _console.Write("> ");

            var line = _console.ReadLine();
            if (line == string.Empty) return false;

            repl.Execute(line);
            return true;
        }
    }
}
