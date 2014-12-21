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
                var repl = new Mock<IRepl>();

                var cmd = new ResetCommand();

                // act
                cmd.Execute(repl.Object, null);

                // assert
                repl.Verify(x => x.Reset(), Times.Once);
            }
        }
    }
}