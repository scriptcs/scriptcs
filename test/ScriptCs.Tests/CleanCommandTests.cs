using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;
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
        }
    }
}
