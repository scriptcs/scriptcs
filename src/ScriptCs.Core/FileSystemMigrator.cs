using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class FileSystemMigrator : IFileSystemMigrator
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;
        private readonly Dictionary<string, string> _fileCopies;
        private readonly Dictionary<string, string> _directoryMoves;
        private readonly Dictionary<string, string> _directoryCopies;

        public FileSystemMigrator(IFileSystem fileSystem, ILog logger)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logger", logger);

            _fileSystem = fileSystem;
            _logger = logger;

            _fileCopies = new Dictionary<string, string>
            {
                { "packages.config", _fileSystem.PackagesFile },
                { "nuget.config", _fileSystem.NugetFile },
            };

            _directoryMoves = new Dictionary<string, string>
            {
                { ".cache", _fileSystem.DllCacheFolder },
            };

            _directoryCopies = new Dictionary<string, string>
            {
                { "bin", _fileSystem.BinFolder },
                { "packages", _fileSystem.PackagesFolder },
            };
        }

        public void Migrate()
        {
            foreach (var copy in _fileCopies
                .Where(copy => _fileSystem.FileExists(copy.Value)))
            {
                _logger.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Not performing migration since file '{0}' already exists.",
                    copy.Value);

                return;
            }

            foreach (var action in _directoryMoves.Concat(_directoryCopies)
                .Where(action => _fileSystem.DirectoryExists(action.Value)))
            {
                _logger.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Not performing migration since directory '{0}' already exists.",
                    action.Value);

                return;
            }

            foreach (var copy in _fileCopies
                .Where(copy => _fileSystem.FileExists(copy.Key)))
            {
                _logger.InfoFormat(
                    CultureInfo.InvariantCulture, "Copying file '{0}' to '{1}'...", copy.Key, copy.Value);

                _fileSystem.CopyFile(copy.Key, copy.Value, false);
            }

            foreach (var move in _directoryMoves
                .Where(move => _fileSystem.DirectoryExists(move.Key)))
            {
                _logger.InfoFormat(
                    CultureInfo.InvariantCulture, "Moving directory '{0}' to '{1}'...", move.Key, move.Value);

                _fileSystem.MoveDirectory(move.Key, move.Value);
            }

            foreach (var copy in _directoryCopies
                .Where(copy => _fileSystem.DirectoryExists(copy.Key)))
            {
                _logger.InfoFormat(
                    CultureInfo.InvariantCulture, "Copying directory '{0}' to '{1}'...", copy.Key, copy.Value);

                _fileSystem.CopyDirectory(copy.Key, copy.Value, false);
            }
        }
    }
}
