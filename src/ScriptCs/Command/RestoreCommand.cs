using System;
using System.IO;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class RestoreCommand : IRestoreCommand
    {
        private readonly string _scriptName;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        private readonly ILog _logger;

        public RestoreCommand(string scriptName, IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver, ILog logger)
        {
            _scriptName = scriptName;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            _logger.Info("Copying assemblies to bin folder...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            var binFolder = Path.Combine(workingDirectory, Constants.BinFolder);

            try
            {
                if (!_fileSystem.DirectoryExists(binFolder))
                    _fileSystem.CreateDirectory(binFolder);

                var packages = _packageAssemblyResolver.GetAssemblyNames(workingDirectory);
                foreach (var package in packages)
                {
                    CopyFile(package, binFolder);
                }

                _logger.Info("Restore completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Restore failed: {0}.", e.Message);
                return CommandResult.Error;
            }
        }

        private void CopyFile(string package, string binFolder)
        {
            var assemblyFileName = Path.GetFileName(package);
            if (assemblyFileName == null) return;

            var destFile = Path.Combine(binFolder, assemblyFileName);

            var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(package);
            var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);

            if (sourceFileLastWriteTime == destFileLastWriteTime)
            {
                _logger.InfoFormat("Skipped: {0}.", assemblyFileName);
                return;
            }

            _fileSystem.Copy(package, destFile, true);

            _logger.InfoFormat("Copied: {0}.", assemblyFileName);
        }
    }
}