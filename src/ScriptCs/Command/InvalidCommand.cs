using PowerArgs;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class InvalidCommand : IInvalidCommand
    {
        private readonly ILog _logger;

        public InvalidCommand(ILog logger)
        {
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.Fatal(ArgUsage.GetUsage<ScriptCsArgs>());
            return CommandResult.Error;
        }
    }
}