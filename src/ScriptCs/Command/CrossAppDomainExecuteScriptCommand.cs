using System;

namespace ScriptCs.Command
{
    [Serializable]
    public class CrossAppDomainExecuteScriptCommand : ICrossAppDomainScriptCommand
    {
        public ScriptCsArgs CommandArgs { get; set; }
        public string[] ScriptArgs { get; set; }
        public CommandResult Result { get; private set; }

        public void Execute()
        {
            if (this.CommandArgs == null)
            {
                throw new InvalidOperationException("The command args are missing.");
            }

            var services = ScriptServicesBuilderFactory.Create(this.CommandArgs, this.ScriptArgs).Build();
            var command = new ExecuteScriptCommand(
                this.CommandArgs.ScriptName,
                this.ScriptArgs,
                services.FileSystem,
                services.Executor,
                services.ScriptPackResolver,
                services.Logger,
                services.AssemblyResolver);

            this.Result = command.Execute();
        }
    }
}
