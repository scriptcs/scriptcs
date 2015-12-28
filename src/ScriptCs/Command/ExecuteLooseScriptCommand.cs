using System;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteLooseScriptCommand : ExecuteScriptCommandBase, IExecuteLooseScriptCommand
    {
        public ExecuteLooseScriptCommand(
            string script, string[] scriptArgs,
            IFileSystem fileSystem, IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILogProvider logProvider,
            IAssemblyResolver assemblyResolver,
            IFileSystemMigrator fileSystemMigrator,
            IScriptLibraryComposer composer) : 
                base(script, scriptArgs, fileSystem, scriptExecutor, scriptPackResolver, logProvider, assemblyResolver, fileSystemMigrator, composer)
        {
        }

        public override CommandResult Execute()
        {
            try
            {
                _fileSystemMigrator.Migrate();

                var assemblyPaths = Enumerable.Empty<string>();
                var workingDirectory = _fileSystem.CurrentDirectory;
                assemblyPaths = _assemblyResolver.GetAssemblyPaths(workingDirectory);
                _composer.Compose(workingDirectory);

                _scriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks());

                // HACK: This is a (dirty) fix for #1086. This might be a temporary solution until some further refactoring can be done. 
                _scriptExecutor.ScriptEngine.CacheDirectory = Path.Combine(workingDirectory ?? _fileSystem.CurrentDirectory, _fileSystem.DllCacheFolder);
                var scriptResult = _scriptExecutor.ExecuteScript(_script, ScriptArgs);
                var commandResult = Inspect(scriptResult);
                _scriptExecutor.Terminate();
                return commandResult;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error executing script '{0}'", ex, _script);
                return CommandResult.Error;
            }
        }
    }
}
