using System;
using System.IO;
using System.Linq;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class CleanCommand : ICleanCommand
    {
        private readonly string _scriptName;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly ILog _logger;

        public CleanCommand(string scriptName, 
            IFileSystem fileSystem, 
            IPackageAssemblyResolver packageAssemblyResolver, ILog logger)
        {
            _scriptName = scriptName;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.Info("Cleaning initiated...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            _logger.TraceFormat("Working directory: {0}", workingDirectory);

            var binFolder = Path.Combine(workingDirectory, Constants.BinFolder);
            _logger.TraceFormat("Bin folder: {0}", binFolder);

            var packageFolder = Path.Combine(workingDirectory, Constants.PackagesFolder);
            _logger.TraceFormat("Packages folder: {0}", packageFolder);

            try
            {
                if (_fileSystem.DirectoryExists(binFolder))
                {
                    var packages = _packageAssemblyResolver.GetAssemblyNames(workingDirectory);

                    foreach (var package in packages)
                    {
                        _logger.DebugFormat("Deleting file: {0}", package);
                        DeleteFile(package, binFolder);
                    }

                    var remaining = _fileSystem.EnumerateFiles(binFolder, "*.*").Any();
                    if (!remaining)
                    {
                        _logger.DebugFormat("Deleting bin directory: {0}", binFolder);
                        _fileSystem.DeleteDirectory(binFolder);
                    }
                }

                if (_fileSystem.DirectoryExists(packageFolder))
                {
                    _logger.DebugFormat("Deleting package directory: {0}", packageFolder);
                    _fileSystem.DeleteDirectory(packageFolder);
                }

                _logger.Info("Clean completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Clean failed: {0}.", e.Message);
                return CommandResult.Error;
            }
        }

        private void DeleteFile(string package, string binFolder)
        {
            var assemblyFileName = Path.GetFileName(package);
            if (assemblyFileName == null) return;

            var destFile = Path.Combine(binFolder, assemblyFileName);

            if (_fileSystem.FileExists(destFile))
            {
                _fileSystem.FileDelete(destFile);
            }
        }
    }
}