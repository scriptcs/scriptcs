using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class ClearCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsClear()
            {
                // act
                var cmd = new ClearCommand(new Mock<IConsole>().Object);

                // assert
                Assert.Equal("clear", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void CallsConsoleClear()
            {
                // arrange
                var console = new Mock<IConsole>();
                var cmd = new ClearCommand(console.Object);

                // act
                cmd.Execute(new Mock<IRepl>().Object, null);

                // assert
                console.Verify(x => x.Clear(), Times.Once);
            }
        }
    }
}