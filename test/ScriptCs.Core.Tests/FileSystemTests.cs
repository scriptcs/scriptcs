﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                const string PathToMyScriptFolder = @"..\my_script";

                try
                {
                    Directory.CreateDirectory(PathToMyScriptFolder);

                    _fileSystem.GetWorkingDirectory(PathToMyScriptFolder)
                              .ShouldEqual(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathToMyScriptFolder)));
                }
                finally
                {
                    if (Directory.Exists(PathToMyScriptFolder))
                    {
                        Directory.Delete(PathToMyScriptFolder);
                    }
                }
            }

            [Fact]
            public void ShouldReturnWorkingDirectoryIfPathIsInvalid()
            {
                var invalidPaths = new List<string> { string.Empty, " ", null };

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
                    {
                        Directory.Delete(existingDirectoryPath);
                    }

                    if (File.Exists(existingFilePath))
                    {
                        File.Delete(existingFilePath);
                    }

                    if (Directory.Exists(workingDir))
                    {
                        Directory.Delete(workingDir);
                    }
                }
            }

            [Fact]
            public void ReturnsCorrectWorkingDirectoryIfPathDoesNotExist()
            {
                const string NonExistantFilePath = @"C:\working_dir\i_dont_exist.txt";

                _fileSystem.GetWorkingDirectory(NonExistantFilePath).ShouldEqual(@"C:\working_dir");
            }
        }

        public class SplitLinesMethod
        {
            private readonly FileSystem _fileSystem = new FileSystem();

            [Fact]
            public void ReturnsCorrectLines()
            {
                var fileContentsLin = "using System;\nusing System.IO;";
                var fileContentsWin = "using System;\r\nusing System.IO;";

                _fileSystem.SplitLines(fileContentsLin).Count().ShouldEqual(2);
                _fileSystem.SplitLines(fileContentsWin).Count().ShouldEqual(2);
            }
        }
    }
}