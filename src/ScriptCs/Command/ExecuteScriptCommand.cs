using System;
using System.IO;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class ExecuteScriptCommand : ExecuteScriptCommandBase, IScriptCommand
    {
        public ExecuteScriptCommand(
            string script, string[] scriptArgs, 
            IFileSystem fileSystem, IScriptExecutor scriptExecutor, 
            IScriptPackResolver scriptPackResolver, 
            ILogProvider logProvider, 
            IAssemblyResolver assemblyResolver, 
            IScriptLibraryComposer composer) : 
                base(script, scriptArgs, fileSystem, scriptExecutor, scriptPackResolver, logProvider, assemblyResolver, composer)
        {
        }

        public override CommandResult Execute()
        {
            try
            {
                var assemblyPaths = Enumerable.Empty<string>();
                var workingDirectory = FileSystem.GetWorkingDirectory(Script);
                if (workingDirectory != null)
                {
                    assemblyPaths = AssemblyResolver.GetAssemblyPaths(workingDirectory);
                }

                Composer.Compose(workingDirectory);

                ScriptExecutor.Initialize(assemblyPaths, _scriptPackResolver.GetPacks(), ScriptArgs);

                // HACK: This is a (dirty) fix for #1086. This might be a temporary solution until some further refactoring can be done. 
                ScriptExecutor.ScriptEngine.CacheDirectory = Path.Combine(workingDirectory ?? FileSystem.CurrentDirectory, FileSystem.DllCacheFolder);
                var scriptResult = ScriptExecutor.Execute(Script, ScriptArgs);

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
