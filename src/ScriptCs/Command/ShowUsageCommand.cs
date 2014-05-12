using Common.Logging;
using PowerArgs;

namespace ScriptCs.Command
{
    public class ShowUsageCommand : ICommand
    {
        private readonly ILog _logger;

        private readonly bool _isValid;

        public ShowUsageCommand(ILog logger, bool isValid)
        {
            _logger = logger;
            _isValid = isValid;
        }

        public CommandResult Execute()
        {
            var options = new ArgUsageOptions { ShowPosition = false, ShowType = false };
            var usage = ArgUsage.GetUsage<ScriptCsArgs>(options: options);

            if (_isValid)
            {
                _logger.Info(usage);
                return CommandResult.Success;
            }

            _logger.Error(usage);
            return CommandResult.Error;
        }
    }
}
