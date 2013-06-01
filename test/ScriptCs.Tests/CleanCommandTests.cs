using System;
using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class CleanCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldDeletePackagesFolder([Frozen] Mock<IFileSystem> fileSystem, CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldDeleteBinFolder([Frozen] Mock<IFileSystem> fileSystem, CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Once());
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotDeleteBinFolderIfDllsAreLeft([Frozen] Mock<IFileSystem> fileSystem, CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:/");
                fileSystem.Setup(i => i.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "c:/file.dll", "c:/file2.dll" });

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.BinFolder))), Times.Never());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldDeleteAllFilesResolvedFromPackages(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IPackageAssemblyResolver> resolver,
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.BinFolder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");
                fileSystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(true);

                resolver.Setup(i => i.GetAssemblyNames(It.IsAny<string>(), It.IsAny<Action<string>>())).Returns(new[] { "c:\\file.dll", "c:\\file2.dll" });

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.FileDelete(It.IsAny<string>()), Times.Exactly(2));
            }
        }
    }
}
