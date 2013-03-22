using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Moq;

using ScriptCs.Command;
using ScriptCs.Contracts;

using Xunit;

namespace ScriptCs.Tests
{
    public class RestoreCommandTests
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void ShouldNotCopyFilesInPathIfLastWriteTimeEqualsLastWriteTimeOfFileInBin()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();
                var packageAssemblyResolver = new Mock<IPackageAssemblyResolver>();

                var command = new RestoreCommand(fileSystem.Object, packageAssemblyResolver.Object);

                var currentDirectory = @"C:\fileDir";

                var sourceFilePath = Path.Combine(@"C:\fileDir", "fileName.cs");
                var sourceWriteTime = new DateTime(2013, 3, 7);

                var destinationFilePath = Path.Combine(currentDirectory, @"C:\fileDir\bin\fileName.cs");
                var destinatioWriteTime = sourceWriteTime;

                packageAssemblyResolver.Setup(par => par.GetAssemblyNames(currentDirectory, It.IsAny<Action<string>>())).Returns(new[] { sourceFilePath });

                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                fileSystem.Setup(fs => fs.GetLastWriteTime(sourceFilePath)).Returns(sourceWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.GetLastWriteTime(destinationFilePath)).Returns(destinatioWriteTime).Verifiable();

                fileSystem.Setup(fs => fs.Copy(sourceFilePath, destinationFilePath, true));

                // act
                command.Execute();

                // assert
                fileSystem.Verify(fs => fs.Copy(sourceFilePath, destinationFilePath, true), Times.Never());
                fileSystem.Verify(fs => fs.GetLastWriteTime(sourceFilePath), Times.Once());
                fileSystem.Verify(fs => fs.GetLastWriteTime(destinationFilePath), Times.Once());
            }

            [Fact]
            public void ShouldCopyFilesInPathIfLastWriteTimeDiffersFromLastWriteTimeOfFileInBin()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();
                var packageAssemblyResolver = new Mock<IPackageAssemblyResolver>();

                var command = new RestoreCommand(fileSystem.Object, packageAssemblyResolver.Object);

                var currentDirectory = @"C:\fileDir";

                var sourceFilePath = Path.Combine(@"C:\fileDir", "fileName.cs");
                var sourceWriteTime = new DateTime(2013, 3, 7);

                var destinationFilePath = Path.Combine(currentDirectory, @"C:\fileDir\bin\fileName.cs");
                var destinatioWriteTime = new DateTime(2013, 3, 8);

                packageAssemblyResolver.Setup(par => par.GetAssemblyNames(currentDirectory, It.IsAny<Action<string>>())).Returns(new[] { sourceFilePath });

                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.GetLastWriteTime(sourceFilePath)).Returns(sourceWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.GetLastWriteTime(destinationFilePath)).Returns(destinatioWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.Copy(sourceFilePath, destinationFilePath, true));

                // act
                command.Execute();

                // assert
                fileSystem.Verify(fs => fs.Copy(sourceFilePath, destinationFilePath, true), Times.Once());
                fileSystem.Verify(fs => fs.GetLastWriteTime(sourceFilePath), Times.Once());
                fileSystem.Verify(fs => fs.GetLastWriteTime(destinationFilePath), Times.Once());
            }

            [Fact]
            public void ShouldCreateCurrentDirectoryIfItDoesNotExist()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var binDirectory = Path.Combine(currentDirectory, "bin");

                fileSystem.Setup(fs => fs.DirectoryExists(binDirectory)).Returns(false).Verifiable();
                fileSystem.Setup(fs => fs.CreateDirectory(binDirectory)).Verifiable();

                var packageAssemblyResolver = new Mock<IPackageAssemblyResolver>();

                var command = new RestoreCommand(fileSystem.Object, packageAssemblyResolver.Object);

                // act
                command.Execute();

                // assert
                fileSystem.Verify(fs => fs.DirectoryExists(binDirectory), Times.Once());
                fileSystem.Verify(fs => fs.CreateDirectory(binDirectory), Times.Once());
            }
        }
    }
}