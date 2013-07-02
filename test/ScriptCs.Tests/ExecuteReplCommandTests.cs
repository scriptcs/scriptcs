using System.Text;
using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;

using Should;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ExecuteReplCommandTests
    {
        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldPromptForInput(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IConsole> console,
                CommandFactory factory)
            {
                // Arrange
                var readLines = 0;
                var builder = new StringBuilder();
                var args = new ScriptCsArgs { Repl = true };

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");

                console.Setup(x => x.ReadLine()).Returns(() => string.Empty).Callback(() => readLines++);
                console.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(value => builder.Append(value));

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                builder.ToString().EndsWith("> ").ShouldBeTrue();
                readLines.ShouldEqual(1);
            }
        }
    }
}
