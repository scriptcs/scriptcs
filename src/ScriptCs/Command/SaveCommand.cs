using System;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class SaveCommand : ISaveCommand
    {
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;
        private readonly IFileSystemMigrator _fileSystemMigrator;

        public SaveCommand(IPackageAssemblyResolver packageAssemblyResolver, IFileSystem fileSystem, ILog logger, IFileSystemMigrator fileSystemMigrator)
        {
            Guard.AgainstNullArgument("packageAssemblyResolver", packageAssemblyResolver);
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);

            _packageAssemblyResolver = packageAssemblyResolver;
            _fileSystem = fileSystem;
            _logger = logger;
            _fileSystemMigrator = fileSystemMigrator;
        }

        public CommandResult Execute()
        {
            _fileSystemMigrator.Migrate();

            _logger.InfoFormat("Saving packages in {0}...", _fileSystem.PackagesFile);

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