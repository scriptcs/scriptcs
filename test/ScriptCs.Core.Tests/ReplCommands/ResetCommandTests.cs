using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class ResetCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsReset()
            {
                // act
                var cmd = new ResetCommand();

                // assert
                Assert.Equal("reset", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void CallsReplReset()
            {
                // arrange
                var executor = new Mock<IScriptExecutor>();

                var cmd = new ResetCommand();

                // act
                var result = cmd.Execute(executor.Object, null);

                // assert
                executor.Verify(x => x.Reset(), Times.Once);
            }
        }
    }
}