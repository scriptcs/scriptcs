using System;
using System.Collections.Generic;
using System.Linq;
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

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public FileSystemMigrator(IFileSystem fileSystem, Common.Logging.ILog logger)
            : this(fileSystem, new CommonLoggingLogProvider(logger))
        {
        }

        public FileSystemMigrator(IFileSystem fileSystem, ILogProvider logProvider)
        {
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logProvider", logProvider);

            _fileSystem = fileSystem;
            _logger = logProvider.ForCurrentType();

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
                    "Not performing migration since file '{0}' already exists.",
                    copy.Value);

                return;
            }

            foreach (var action in _directoryMoves.Concat(_directoryCopies)
                .Where(action => _fileSystem.DirectoryExists(action.Value)))
            {
                _logger.DebugFormat(
                    "Not performing migration since directory '{0}' already exists.",
                    action.Value);

                return;
            }

            foreach (var copy in _fileCopies
                .Where(copy => _fileSystem.FileExists(copy.Key)))
            {
                _logger.InfoFormat(
                    "Copying file '{0}' to '{1}'...", copy.Key, copy.Value);

                _fileSystem.Copy(copy.Key, copy.Value, false);
            }

            foreach (var move in _directoryMoves
                .Where(move => _fileSystem.DirectoryExists(move.Key)))
            {
                _logger.InfoFormat(
                    "Moving directory '{0}' to '{1}'...", move.Key, move.Value);

                _fileSystem.MoveDirectory(move.Key, move.Value);
            }

            foreach (var copy in _directoryCopies
                .Where(copy => _fileSystem.DirectoryExists(copy.Key)))
            {
                _logger.InfoFormat(
                    "Copying directory '{0}' to '{1}'...", copy.Key, copy.Value);

                _fileSystem.CopyDirectory(copy.Key, copy.Value, false);
            }
        }
    }
}
