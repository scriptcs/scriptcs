using System.Collections.Generic;
using System.Globalization;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class MigrateCommand : IMigrateCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;

        public MigrateCommand(IFileSystem fileSystem, ILog logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            var folderMigrations = new Dictionary<string, string>
            {
                { "bin", _fileSystem.BinFolder },
                { ".cache", _fileSystem.DllCacheFolder },
                { "packages", _fileSystem.PackagesFolder },
            };

            var fileMigrations = new Dictionary<string, string>
            {
                { "packages.config", _fileSystem.PackagesFile },
                { "nuget.config", _fileSystem.NugetFile },
            };

            foreach (var migration in folderMigrations)
            {
                if (_fileSystem.DirectoryExists(migration.Key))
                {
                    _logger.InfoFormat(
                        CultureInfo.InvariantCulture, "Moving folder '{0}' to '{1}'.", migration.Key, migration.Value);

                    _fileSystem.MoveFolder(migration.Key, migration.Value);
                }
            }

            foreach (var migration in fileMigrations)
            {
                if (_fileSystem.FileExists(migration.Key))
                {
                    _logger.InfoFormat(
                        CultureInfo.InvariantCulture, "Moving file '{0}' to '{1}'.", migration.Key, migration.Value);

                    _fileSystem.Move(migration.Key, migration.Value);
                }
            }

            _logger.Info("Migration complete.");

            return CommandResult.Success;
        }
    }
}
