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
                FileSystemMigrator.Migrate();

                var assemblyPaths = Enumerable.Empty<string>();
                var workingDirectory = FileSystem.CurrentDirectory;
                assemblyPaths = AssemblyResolver.GetAssemblyPaths(workingDirectory);
                Composer.Compose(workingDirectory);

                ScriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks());

                // HACK: This is a (dirty) fix for #1086. This might be a temporary solution until some further refactoring can be done. 
                ScriptExecutor.ScriptEngine.CacheDirectory = Path.Combine(workingDirectory ?? FileSystem.CurrentDirectory, FileSystem.DllCacheFolder);
                var scriptResult = ScriptExecutor.ExecuteScript(Script, ScriptArgs);
                var commandResult = Inspect(scriptResult);
                ScriptExecutor.Terminate();
                return commandResult;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error executing script '{0}'", ex, Script);
                return CommandResult.Error;
            }
        }
    }
}
