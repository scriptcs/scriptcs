using System;
using System.IO;

namespace ScriptCs.Command
{
    internal class RestoreCommand : IRestoreCommand
    {
        private readonly string _scriptName;

        private readonly IFileSystem _fileSystem;

        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        public RestoreCommand(string scriptName, IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver)
        {
            _scriptName = scriptName;
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
        }

        public CommandResult Execute()
        {
            Console.WriteLine("Copying assemblies to bin folder...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            var binFolder = Path.Combine(workingDirectory, "bin");

            try
            {
                if (!_fileSystem.DirectoryExists(binFolder))
                    _fileSystem.CreateDirectory(binFolder);

                var packages = _packageAssemblyResolver.GetAssemblyNames(workingDirectory);
                foreach (var package in packages)
                {
                    CopyFile(package, binFolder);
                }

                Console.WriteLine("Restore completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine("Restore failed: {0}.", e.Message);
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
                Console.WriteLine("Skipped: {0}.", assemblyFileName);
                return;
            }

            _fileSystem.Copy(package, destFile, true);

            Console.WriteLine("Copied: {0}.", assemblyFileName);
        }
    }
}