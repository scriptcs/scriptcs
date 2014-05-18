using System;

namespace ScriptCs.Command
{
    [Serializable]
    public class CrossAppDomainExecuteScriptCommand
    {
        private readonly ScriptCsArgs _commandArgs;
        private readonly string[] _scriptArgs;

        public CrossAppDomainExecuteScriptCommand(ScriptCsArgs commandArgs, string[] scriptArgs)
        {
            Guard.AgainstNullArgument("commandArgs", commandArgs);

            _commandArgs = commandArgs;
            _scriptArgs = scriptArgs;
        }

        public void Execute()
        {
            var services = ScriptServicesBuilderFactory.Create(_commandArgs, _scriptArgs).Build();
            var command = new ExecuteScriptCommand(
                _commandArgs.ScriptName,
                _scriptArgs,
                services.FileSystem,
                services.Executor,
                services.ScriptPackResolver,
                services.Logger,
                services.AssemblyResolver);

            command.Execute();
        }
    }
}
