using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Command;
using ScriptCs.Contracts;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    using Common.Logging;

    public class SaveCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void MigratesTheFileSystem(
                [Frozen] Mock<IFileSystem> fileSystem, [Frozen] Mock<IFileSystemMigrator> migrator)
            {
                // Arrange
                var sut = new SaveCommand(
                    new Mock<IPackageAssemblyResolver>().Object,
                    fileSystem.Object,
                    new Mock<ILog>().Object,
                    migrator.Object);

                // Act
                sut.Execute();

                // Assert
                migrator.Verify(m => m.Migrate(), Times.Once);
            }
        }
    }
}
