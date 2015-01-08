using Common.Logging;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class CleanCommandTests
    {
        public class ExecuteMethod
        {
            [ScriptCsAutoData("scriptcs_packages")]
            [ScriptCsAutoData(".scriptcs_cache")]
            [Theory]
            public void ShouldDeletePackagesFolder(string folder, 
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IFileSystemMigrator> fileSystemMigrator,
                ScriptServices services)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(folder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                servicesBuilder.Setup(b => b.Build()).Returns(services);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                var factory = new CommandFactory(servicesBuilder.Object);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(folder))), Times.Once());
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(folder))), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void MigratesTheFileSystem(
                [Frozen] Mock<IFileSystem> fileSystem, [Frozen] Mock<IFileSystemMigrator> migrator)
            {
                // Arrange
                var sut = new CleanCommand(null, fileSystem.Object, new Mock<ILog>().Object, migrator.Object);

                // Act
                sut.Execute();

                // Assert
                migrator.Verify(m => m.Migrate(), Times.Once);
            }
        }
    }
}
