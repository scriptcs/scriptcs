using System;
using System.IO;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class CleanCommand : ICleanCommand
    {
        private readonly string _scriptName;
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;

        public CleanCommand(string scriptName, IFileSystem fileSystem, ILogProvider logProvider)
        {
            Guard.AgainstNullArgumentProperty("fileSystem", "PackagesFolder", fileSystem.PackagesFolder);
            Guard.AgainstNullArgumentProperty("fileSystem", "DllCacheFolder", fileSystem.DllCacheFolder);

            Guard.AgainstNullArgument("logProvider", logProvider);

            _scriptName = scriptName;
            _fileSystem = fileSystem;
            _logger = logProvider.ForCurrentType();
        }

        public CommandResult Execute()
        {
            _logger.Info("Cleaning initiated...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            _logger.TraceFormat("Working directory: {0}", workingDirectory);

            var packageFolder = Path.Combine(workingDirectory, _fileSystem.PackagesFolder);
            _logger.TraceFormat("Packages folder: {0}", packageFolder);

            var cacheFolder = Path.Combine(workingDirectory, _fileSystem.DllCacheFolder);
            _logger.TraceFormat("Cache folder: {0}", cacheFolder);

            try
            {
                if (_fileSystem.DirectoryExists(packageFolder))
                {
                    _logger.DebugFormat("Deleting package directory: {0}", packageFolder);
                    _fileSystem.DeleteDirectory(packageFolder);
                }

                if (_fileSystem.DirectoryExists(cacheFolder))
                {
                    _logger.DebugFormat("Deleting cache directory: {0}", cacheFolder);
                    _fileSystem.DeleteDirectory(cacheFolder);
                }

                _logger.Info("Cleaning completed.");
                return CommandResult.Success;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Cleaning failed: {0}.", ex, ex.Message);
                return CommandResult.Error;
            }
        }
    }
}
