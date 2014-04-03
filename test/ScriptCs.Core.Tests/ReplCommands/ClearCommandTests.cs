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
                var console = new Mock<IConsole>();
                var cmd = new ClearCommand(console.Object);
                Assert.Equal("clear", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void CallsConsoleClear()
            {
                var console = new Mock<IConsole>();
                var executor = new Mock<IScriptExecutor>();

                var cmd = new ClearCommand(console.Object);
                var result = cmd.Execute(executor.Object, null);

                console.Verify(x => x.Clear(), Times.Once);
            }
        }
    }
}