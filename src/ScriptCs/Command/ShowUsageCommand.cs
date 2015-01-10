using Common.Logging;
using PowerArgs;

namespace ScriptCs.Command
{
    internal class ShowUsageCommand : IHelpCommand, IInvalidCommand
    {
        private readonly ILog _logger;

        public ShowUsageCommand(ILog logger)
        {
            Guard.AgainstNullArgument("logger", logger);

            _logger = logger;
        }

        public CommandResult Execute()
        {
            var options = new ArgUsageOptions { ShowPosition = false, ShowType = false };
            var usage = ArgUsage.GetUsage<ScriptCsArgs>(options: options);
            _logger.Info(usage);
            return CommandResult.Success;
        }
    }
}
