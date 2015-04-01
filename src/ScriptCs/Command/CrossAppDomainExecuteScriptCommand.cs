using System;

namespace ScriptCs.Command
{
    [Serializable]
    public class CrossAppDomainExecuteScriptCommand : ICrossAppDomainScriptCommand
    {
        public Config Config { get; set; }
        public string[] ScriptArgs { get; set; }
        public CommandResult Result { get; private set; }

        public void Execute()
        {
            if (this.Config == null)
            {
                throw new InvalidOperationException("The config is missing.");
            }

            var services = ScriptServicesBuilderFactory.Create(this.Config, this.ScriptArgs).Build();
            var command = new ExecuteScriptCommand(
                this.Config.ScriptName,
                this.ScriptArgs,
                services.FileSystem,
                services.Executor,
                services.ScriptPackResolver,
                services.LogProvider,
                services.AssemblyResolver,
                services.FileSystemMigrator,
                services.ScriptLibraryComposer);

            this.Result = command.Execute();
        }
    }
}
