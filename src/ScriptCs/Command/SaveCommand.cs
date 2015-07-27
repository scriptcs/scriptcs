using System;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class SaveCommand : ISaveCommand
    {
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;

        public SaveCommand(IPackageAssemblyResolver packageAssemblyResolver, IFileSystem fileSystem, ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("packageAssemblyResolver", packageAssemblyResolver);
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logProvider", logProvider);

            _packageAssemblyResolver = packageAssemblyResolver;
            _fileSystem = fileSystem;
            _logger = logProvider.ForCurrentType();
        }

        public CommandResult Execute()
        {
            _logger.InfoFormat("Saving packages in {0}...", _fileSystem.PackagesFile);

            try
            {
                _packageAssemblyResolver.SavePackages();
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Package saving failed: {0}.", ex, ex.Message);
                return CommandResult.Error;
            }

            return CommandResult.Success;
        }
    }
}