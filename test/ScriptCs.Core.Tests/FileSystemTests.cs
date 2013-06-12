using System;
using System.Collections.Generic;
using System.IO;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class FileSystemTests
    {
        public class GetWorkingDirectoryMethod
        {
            private readonly FileSystem _fileSystem = new FileSystem();

            [Fact]
            public void ShouldProperlyConstructWorkingDirectoryIfScriptIsRunFromRelativePath()
            {
                const string pathToMyScriptFolder = @"..\my_script";

                try
                {
                    Directory.CreateDirectory(pathToMyScriptFolder);

                    _fileSystem.GetWorkingDirectory(pathToMyScriptFolder)
                              .ShouldEqual(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, pathToMyScriptFolder)));
                }
                finally
                {
                    if (Directory.Exists(pathToMyScriptFolder))
                        Directory.Delete(pathToMyScriptFolder);
                }
            }

            [Fact]
            public void ShouldReturnWorkingDirectoryIfPathIsInvalid()
            {
                var invalidPaths = new List<string> {"", " ", null};

                foreach (var invalidPath in invalidPaths)
                {
                   _fileSystem.GetWorkingDirectory(invalidPath).ShouldEqual(_fileSystem.CurrentDirectory);
                }
            }

            [Fact]
            public void ReturnsCorrectWorkingDirectory()
            {
                string workingDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @".\working_dir\"));
                string existingDirectoryPath = Path.GetFullPath(Path.Combine(workingDir, @".\existing_dir\"));
                string existingFilePath = Path.GetFullPath(Path.Combine(workingDir, @".\existing_file.txt"));

                try
                {
                    Directory.CreateDirectory(workingDir);
                    Directory.CreateDirectory(existingDirectoryPath);
                    File.Create(existingFilePath).Dispose();

                    _fileSystem.GetWorkingDirectory(existingDirectoryPath).ShouldEqual(existingDirectoryPath);
                    _fileSystem.GetWorkingDirectory(existingFilePath).ShouldEqual(
                        Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @".\working_dir")));
                }
                finally
                {
                    if (Directory.Exists(existingDirectoryPath))
                        Directory.Delete(existingDirectoryPath);

                    if (File.Exists(existingFilePath))
                        File.Delete(existingFilePath);

                    if (Directory.Exists(workingDir))
                        Directory.Delete(workingDir);
                }
            }

            [Fact]
            public void ReturnsCorrectWorkingDirectoryIfPathDoesNotExist()
            {
                const string nonExistantFilePath = @"C:\working_dir\i_dont_exist.txt";

                _fileSystem.GetWorkingDirectory(nonExistantFilePath).ShouldEqual(@"C:\working_dir");
            }
        }
    }
}