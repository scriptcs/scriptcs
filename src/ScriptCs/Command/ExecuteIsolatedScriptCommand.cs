using System;
using System.IO;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
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
            var appDomainName = Path.Combine(workingDirectory ?? string.Empty, Script).ToString();
            var setupInfo = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
            bool mustRun;
            AppDomain appDomain;
            do
            {
                mustRun = false;
                appDomain = AppDomain.CreateDomain(appDomainName, null, setupInfo);
                try
                {
                    FileSystem.OnChanged(GetScriptFolder(), Script, () =>
                    {
                        mustRun = true;
                        Unload(appDomain);
                    });
                    appDomain.DoCallBack(IsolatedHelper.Execute);
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
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
                    Logger.Info("Domain already unloaded " + appDomain.FriendlyName);
                    return;
                }
                AppDomain.Unload(appDomain);
            }
            catch(Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private string GetScriptFolder()
        {
            var scriptFolder = Path.GetDirectoryName(Script);
            if(string.IsNullOrEmpty(scriptFolder))
            {
                scriptFolder = FileSystem.CurrentDirectory;
            }
            return scriptFolder;
        }
    }
}