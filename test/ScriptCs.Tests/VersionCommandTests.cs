using System.Diagnostics;

using Moq;
using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class VersionCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void VersionCommandShouldOutputVersion(
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IFileSystem> fileSystem)
            {
                // Arrange
                var args = new ScriptCsArgs { Version = true };

                var assembly = typeof(ScriptCsArgs).Assembly;
                var currentVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

                servicesBuilder.SetupGet(b => b.ConsoleInstance).Returns(console.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);

                var factory = new CommandFactory(servicesBuilder.Object);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                console.Verify(x => x.Write(It.Is<string>(y => y.Contains(currentVersion.ToString()))));
            }
        }
    }
}
