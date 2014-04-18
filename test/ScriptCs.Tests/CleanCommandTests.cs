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
            [ScriptCsAutoData(Constants.PackagesFolder)]
            [ScriptCsAutoData(Constants.DllCacheFolder)]
            [Theory]
            public void ShouldDeletePackagesFolder(string folder, 
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                [Frozen] Mock<InitializationServices> initializationServices,
                ScriptServices services,
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");
                servicesBuilder.Setup(b => b.Build()).Returns(services);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
            }
        }
    }
}
