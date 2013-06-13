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
            var appDomain = AppDomain.CreateDomain(appDomainName, null, setupInfo);
            try
            {
                appDomain.DoCallBack(IsolatedHelper.Execute);
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
            return IsolatedHelper.Result;
        }
    }
}