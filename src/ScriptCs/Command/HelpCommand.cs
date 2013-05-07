using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using PowerArgs;

namespace ScriptCs.Command
{
    internal class HelpCommand : IHelpCommand
    {
        private readonly ILog _logger;

        public HelpCommand(ILog logger)
        {
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.Info(ArgUsage.GetUsage<ScriptCsArgs>());
            return CommandResult.Success;
        }
    }
}
