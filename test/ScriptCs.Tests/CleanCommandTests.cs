using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
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
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // Arrange
                var args = new Config { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(folder)))).Returns(true);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(folder))), Times.Once());
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(folder))), Times.Once());
            }
        }
    }
}
