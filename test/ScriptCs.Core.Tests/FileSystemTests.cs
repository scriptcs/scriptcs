using System;
using System.IO;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class FileSystemTests
    {
        public class GetWorkingDirectoryMethod
        {
            [Fact]
            public void ShouldProperlyConstructWorkingDirectoryIfScriptIsRunFromRelativePath()
            {
                const string pathToMyScriptFolder = @"..\my_script\";

                try
                {
                    Directory.CreateDirectory(pathToMyScriptFolder);

                    var fileSystem = new FileSystem();

                    fileSystem.GetWorkingDirectory(pathToMyScriptFolder)
                              .ShouldEqual(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, pathToMyScriptFolder)));
                }
                finally
                {
                    if (Directory.Exists(pathToMyScriptFolder))
                        Directory.Delete(pathToMyScriptFolder);
                }
            }
        }
    }
}