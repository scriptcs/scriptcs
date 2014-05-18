using System;
using System.IO;
using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    public class CleanCommand : ICommand
    {
        private readonly string _scriptName;

        private readonly IFileSystem _fileSystem;

        private readonly ILog _logger;

        public CleanCommand(string scriptName, IFileSystem fileSystem, ILog logger)
        {
            _scriptName = scriptName;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.Info("Cleaning initiated...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            _logger.TraceFormat("Working directory: {0}", workingDirectory);

            var packageFolder = Path.Combine(workingDirectory, Constants.PackagesFolder);
            _logger.TraceFormat("Packages folder: {0}", packageFolder);

            var cacheFolder = Path.Combine(workingDirectory, Constants.DllCacheFolder);
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

                _logger.Info("Clean completed.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Clean failed: {0}.", e.Message);
                return CommandResult.Error;
            }
        }
    }
}