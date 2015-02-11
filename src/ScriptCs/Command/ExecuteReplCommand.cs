using System;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteReplCommand : IExecuteReplCommand
    {
        private readonly string _scriptName;
        private readonly string[] _scriptArgs;
        private readonly IFileSystem _fileSystem;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IRepl _repl;
        private readonly ILog _logger;
        private readonly IConsole _console;
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly IFileSystemMigrator _fileSystemMigrator;

        public ExecuteReplCommand(
            string scriptName,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptPackResolver scriptPackResolver,
            IRepl repl,
            ILog logger,
            IConsole console,
            IAssemblyResolver assemblyResolver,
            IFileSystemMigrator fileSystemMigrator)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("scriptPackResolver", scriptPackResolver);
            Guard.AgainstNullArgument("repl", repl);
            Guard.AgainstNullArgument("logger", logger);
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("assemblyResolver", assemblyResolver);
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);

            _scriptName = scriptName;
            _scriptArgs = scriptArgs;
            _fileSystem = fileSystem;
            _scriptPackResolver = scriptPackResolver;
            _repl = repl;
            _logger = logger;
            _console = console;
            _assemblyResolver = assemblyResolver;
            _fileSystemMigrator = fileSystemMigrator;
        }

        public string[] ScriptArgs
        {
            get { return _scriptArgs; }
        }

        public CommandResult Execute()
        {
            _fileSystemMigrator.Migrate();

            _console.WriteLine("scriptcs (ctrl-c to exit or :help for help)" + Environment.NewLine);

            var workingDirectory = _fileSystem.CurrentDirectory;
            var assemblies = _assemblyResolver.GetAssemblyPaths(workingDirectory);
            var scriptPacks = _scriptPackResolver.GetPacks();

            _repl.Initialize(assemblies, scriptPacks, ScriptArgs);

            try
            {
                if (!string.IsNullOrWhiteSpace(_scriptName))
                {
                    _logger.Info(string.Format("Loading script: {0}", _scriptName));
                    _repl.Execute(string.Format("#load {0}", _scriptName));
                }

                while (ExecuteLine(_repl))
                {
                }

                _console.WriteLine();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return CommandResult.Error;
            }

            _repl.Terminate();
            return CommandResult.Success;
        }

        private bool ExecuteLine(IRepl repl)
        {
            _console.Write(string.IsNullOrWhiteSpace(repl.Buffer) ? "> " : "* ");

            try
            {
                var line = _console.ReadLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    repl.Execute(line);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
