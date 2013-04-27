using System;
using System.IO;
using System.Linq;

namespace ScriptCs.Command
{
    internal class UncacheCommand : ICleanCommand
    {
        private readonly string _scriptName;
        private readonly IFileSystem _fileSystem;

        public UncacheCommand(string scriptName, IFileSystem fileSystem)
        {
            _scriptName = scriptName;
            _fileSystem = fileSystem;
        }

        public CommandResult Execute()
        {
            Console.WriteLine("Uncache Assembly initiated...");

            var workingDirectory = _fileSystem.GetWorkingDirectory(_scriptName);
            var binFolder = Path.Combine(workingDirectory, Constants.BinFolder);
            var exeFilename = Path.Combine(binFolder, Path.GetFileNameWithoutExtension(_scriptName) + ".dll");
            var pdbFilename = Path.Combine(binFolder, Path.GetFileNameWithoutExtension(_scriptName) + ".pdb");

            try
            {
                if (File.Exists(exeFilename))
                    File.Delete(exeFilename);

                if (File.Exists(pdbFilename))
                    File.Delete(pdbFilename);

                var binFiles = Directory.GetFiles(binFolder);
                if (binFiles.Length == 0)
                    Directory.Delete(binFolder);

                Console.WriteLine("Uncache Assembly completed successfully.");
                return CommandResult.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine("Uncache Assembly failed: {0}.", e.Message);
                return CommandResult.Error;
            }
        }
    }
}
