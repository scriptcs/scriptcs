using System;
using System.IO;
using System.Linq;

namespace ScriptCs.Command
{
    internal class CleanCommand : ICleanCommand
    {
        private readonly string _scriptName;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        public CleanCommand(string scriptName, IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver)
        {
            _scriptName = scriptName;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
        }

        public CommandResult Execute()
        {
            Console.WriteLine("Cleaning initiated...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            var binFolder = Path.Combine(workingDirectory, Constants.BinFolder);
            var packageFolder = Path.Combine(workingDirectory, Constants.PackagesFolder);

            try
            {
                if (_fileSystem.DirectoryExists(binFolder))
                {
                    var packages = _packageAssemblyResolver.GetAssemblyNames(workingDirectory);

                    foreach (var package in packages)
                    {
                        DeleteFile(package, binFolder);
                    }

                    var remaining = _fileSystem.EnumerateFiles(binFolder, "*.*").Any();
                    if (!remaining)
                    {
                        _fileSystem.DeleteDirectory(binFolder);
                    }
                }

                if (_fileSystem.DirectoryExists(packageFolder))
                    _fileSystem.DeleteDirectory(packageFolder);

                Console.WriteLine("Clean completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine("Clean failed: {0}.", e.Message);
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