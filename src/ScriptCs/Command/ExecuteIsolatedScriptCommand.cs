using System;
using System.IO;
using Common.Logging;

namespace ScriptCs.Command
{
    public interface IIsolatedHelper
    {
        ScriptCsArgs CommandArgs { get; set; }
        string[] AssemblyPaths { get; set; }
        string Script { get; set; }
        string[] ScriptArgs { get; set; }
        ScriptResult Result { get; set; }
        
        void Execute();
    }

    internal class ExecuteIsolatedScriptCommand : ExecuteScriptCommand, IIsolatedScriptCommand
    {
        public ExecuteIsolatedScriptCommand(string script,
            string[] scriptArgs,
            IFileSystem fileSystem,
            IScriptExecutor scriptExecutor,
            IScriptPackResolver scriptPackResolver,
            ILog logger,
            IAssemblyResolver assemblyResolver)
            : base(script, scriptArgs, fileSystem, scriptExecutor, scriptPackResolver, logger, assemblyResolver)
        {
        }

        public IIsolatedHelper IsolatedHelper { get; set; }

        protected override ScriptResult Execute(string workingDirectory, string[] assemblyPaths)
        {
            IsolatedHelper.AssemblyPaths = assemblyPaths;
            var appDomainName = Path.Combine(workingDirectory ?? string.Empty, _script).ToString();
            var setupInfo = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
            bool mustRun;
            AppDomain appDomain;
            do
            {
                mustRun = false;
                appDomain = AppDomain.CreateDomain(appDomainName, null, setupInfo);
                try
                {
                    _fileSystem.OnChanged(GetScriptFolder(), _script, () =>
                    {
                        mustRun = true;
                        Unload(appDomain);
                    });
                    appDomain.DoCallBack(IsolatedHelper.Execute);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                }
                finally
                {
                    if(!mustRun)
                    {
                        Unload(appDomain);
                    }
                }
            }while(mustRun);
            return IsolatedHelper.Result;
        }

        private void Unload(AppDomain appDomain)
        {
            try
            {
                if(appDomain.IsFinalizingForUnload())
                {
                    _logger.Info("Domain already unloaded " + appDomain.FriendlyName);
                    return;
                }
                AppDomain.Unload(appDomain);
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private string GetScriptFolder()
        {
            var scriptFolder = Path.GetDirectoryName(_script);
            if(string.IsNullOrEmpty(scriptFolder))
            {
                scriptFolder = _fileSystem.CurrentDirectory;
            }
            return scriptFolder;
        }
    }
}