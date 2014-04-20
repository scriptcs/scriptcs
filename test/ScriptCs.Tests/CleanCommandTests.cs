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
            [ScriptCsAutoData(Constants.PackagesFolder)]
            [ScriptCsAutoData(Constants.DllCacheFolder)]
            [Theory]
            public void ShouldDeletePackagesFolder(string folder, 
                Mock<IFileSystem> fileSystem,
                Mock<IInitializationServices> initializationServices)
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true };
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var servicesBuilder = fixture.Freeze<Mock<IScriptServicesBuilder>>();

                fileSystem.Setup(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder)))).Returns(true);
                fileSystem.Setup(i => i.GetWorkingDirectory(It.IsAny<string>())).Returns("c:\\");

                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                var factory = fixture.Create<CommandFactory>();

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(i => i.DirectoryExists(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
                fileSystem.Verify(i => i.DeleteDirectory(It.Is<string>(x => x.Contains(Constants.PackagesFolder))), Times.Once());
            }
        }
    }
}
