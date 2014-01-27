using System;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class SaveCommand : ISaveCommand
    {
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly ILog _logger;

        public SaveCommand(IPackageAssemblyResolver packageAssemblyResolver, ILog logger)
        {
            _packageAssemblyResolver = packageAssemblyResolver;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.InfoFormat("Saving packages in {0}...", Constants.PackagesFile);

            try
            {
                _packageAssemblyResolver.SavePackages();
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Save failed: {0}.", e, e.Message);
                return CommandResult.Error;
            }

            return CommandResult.Success;
        }
    }
}