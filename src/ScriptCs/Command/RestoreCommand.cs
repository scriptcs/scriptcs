using System;
using System.IO;

namespace ScriptCs.Command
{
    public class RestoreCommand : IRestoreCommand
    {
        private readonly IFileSystem _fileSystem;

        private readonly IPackageAssemblyResolver _packageAssemblyResolver;

        public RestoreCommand(IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver)
        {
            _fileSystem = fileSystem;
            _packageAssemblyResolver = packageAssemblyResolver;
        }

        public virtual int Execute()
        {
            Console.WriteLine("Moving assemblies to bin folder...");

            var binFolder = Path.Combine(_fileSystem.CurrentDirectory, "bin");

            try
            {
                if (!_fileSystem.DirectoryExists(binFolder))
                    _fileSystem.CreateDirectory(binFolder);

                var packages = _packageAssemblyResolver.GetAssemblyNames(_fileSystem.CurrentDirectory);
                foreach (var package in packages)
                {
                    MoveFile(package, binFolder);
                }

                Console.WriteLine("Restore completed successfully.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Restore failed: {0}.", e.Message);
                return -1;
            }
        }

        private void MoveFile(string package, string binFolder)
        {
            var packageFileName = Path.GetFileName(package);
            if (packageFileName == null) return;

            var destFile = Path.Combine(binFolder, packageFileName);

            var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(package);
            var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);

            if (sourceFileLastWriteTime == destFileLastWriteTime)
            {
                Console.WriteLine("Skipped {0}.", packageFileName);
                return;
            }

            _fileSystem.Copy(package, destFile, true);

            Console.WriteLine("Moved {0}.", packageFileName);
        }
    }
}